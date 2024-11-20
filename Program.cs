using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

class Program
{
    static ConsoleColor activeColor = ConsoleColor.White;
    static char drawingChar = '█';
    static List<Point> drawingPoints = new List<Point>();

    static void Main()
    {
        using (var dbContext = new DrawingDbContext())
        {
            dbContext.Database.Migrate();
        }

        string[] menuItems = { "Create", "Edit", "Delete", "View All Drawings", "Exit" };
        int selectedIndex = 0;
        int menuHeight = menuItems.Length + 2; 
        int menuWidth = 20; 

        Console.CursorVisible = false;

        while (true)
        {
            DrawMenuWithBorder(menuItems, menuHeight, menuWidth, selectedIndex);
            var input = Console.ReadKey(true);

            if (input.Key == ConsoleKey.UpArrow)
            {
                selectedIndex = (selectedIndex == 0) ? menuItems.Length - 1 : selectedIndex - 1;
            }
            else if (input.Key == ConsoleKey.DownArrow)
            {
                selectedIndex = (selectedIndex == menuItems.Length - 1) ? 0 : selectedIndex + 1;
            }
            else if (input.Key == ConsoleKey.Enter)
            {
                if (menuItems[selectedIndex] == "Create")
                {
                    EnterDrawingMode();
                }
                else if (menuItems[selectedIndex] == "Edit")
                {
                    EditDrawing();
                }
                else if (menuItems[selectedIndex] == "Delete")
                {
                    DeleteDrawing();
                }
                else if (menuItems[selectedIndex] == "View All Drawings")
                {
                    ViewAllDrawings(); 
                }
                else if (menuItems[selectedIndex] == "Exit")
                {
                    Console.Clear();
                    Console.WriteLine("Exiting..."); 
                    break; 
                }
            }
        }
    }

    static void DrawMenuWithBorder(string[] menuItems, int menuHeight, int menuWidth, int selectedIndex)
    {
        int menuStartX = (Console.WindowWidth - menuWidth) / 2;
        int menuStartY = (Console.WindowHeight - menuHeight) / 2;

        Console.Clear();

        Console.SetCursorPosition(menuStartX, menuStartY);
        Console.ForegroundColor = ConsoleColor.White; 
        Console.WriteLine("╔" + new string('═', menuWidth - 2) + "╗");

        for (int i = 0; i < menuItems.Length; i++)
        {
            Console.SetCursorPosition(menuStartX, menuStartY + i + 1);

            if (i == selectedIndex)
            {
                Console.ForegroundColor = ConsoleColor.Green; 
                Console.WriteLine("║" + menuItems[i].PadLeft((menuWidth - 2 + menuItems[i].Length) / 2).PadRight(menuWidth - 2) + "║");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Gray; 
                Console.WriteLine("║" + menuItems[i].PadLeft((menuWidth - 2 + menuItems[i].Length) / 2).PadRight(menuWidth - 2) + "║");
            }
        }

        Console.SetCursorPosition(menuStartX, menuStartY + menuHeight - 1);
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("╚" + new string('═', menuWidth - 2) + "╝");

        Console.ResetColor(); 
    }

    static void EnterDrawingMode()
    {
        Console.Clear();

        int drawingWidth = Console.WindowWidth - 4; 
        int drawingHeight = Console.WindowHeight - 4; 
        int cursorX = Console.WindowWidth / 2;
        int cursorY = Console.WindowHeight / 2;

        drawingPoints.Clear(); 
        DrawDrawingBorder(drawingWidth, drawingHeight);

        ConsoleKeyInfo input;

        while (true)
        {
            Console.SetCursorPosition(cursorX, cursorY);
            Console.ForegroundColor = activeColor;
            Console.Write(drawingChar);
            Console.ResetColor();

            input = Console.ReadKey(true);

            if (input.Key == ConsoleKey.LeftArrow && cursorX > 2) cursorX--;
            else if (input.Key == ConsoleKey.RightArrow && cursorX < drawingWidth + 1) cursorX++;
            else if (input.Key == ConsoleKey.UpArrow && cursorY > 2) cursorY--;
            else if (input.Key == ConsoleKey.DownArrow && cursorY < drawingHeight + 1) cursorY++;

            else if (input.Key == ConsoleKey.Spacebar)
            {
                drawingPoints.Add(new Point { X = cursorX, Y = cursorY, Character = drawingChar, Color = activeColor });
            }

            else if (input.Key >= ConsoleKey.D1 && input.Key <= ConsoleKey.D9)
            {
                int colorIndex = input.Key - ConsoleKey.D1; 
                activeColor = (ConsoleColor)(colorIndex + 1); 
            }

            else if (input.Key == ConsoleKey.Enter)
            {
                Console.Clear();
                Console.WriteLine("Enter a name for your drawing:");
                string drawingName = Console.ReadLine();

                SaveDrawing(drawingName);
                Console.WriteLine($"Drawing '{drawingName}' saved.");
                Console.WriteLine("Press any key to return to the menu...");
                Console.ReadKey();
                break;
            }

            else if (input.Key == ConsoleKey.Escape)
            {
                break;
            }
        }
    }

    static void SaveDrawing(string name)
    {
        using (var dbContext = new DrawingDbContext())
        {
            var drawing = new Drawing
            {
                Name = name,
                Points = new List<Point>(drawingPoints)
            };

            dbContext.Drawings.Add(drawing);
            dbContext.SaveChanges();
        }
    }

    static void EditDrawing()
    {
        using (var dbContext = new DrawingDbContext())
        {
            var drawings = dbContext.Drawings.ToList();

            if (drawings.Count == 0)
            {
                Console.Clear();
                Console.WriteLine("No drawings available to edit.");
                Console.WriteLine("Press any key to return to the menu...");
                Console.ReadKey();
                return;
            }

            Console.Clear();
            Console.WriteLine("Select a drawing to edit:");

            for (int i = 0; i < drawings.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {drawings[i].Name}");
            }

            int drawingIndex = -1;
            while (drawingIndex < 0 || drawingIndex >= drawings.Count)
            {
                Console.Write("Enter the number of the drawing to edit: ");
                if (int.TryParse(Console.ReadLine(), out drawingIndex))
                {
                    drawingIndex--; 
                }
            }

            var selectedDrawing = drawings[drawingIndex];
            drawingPoints = selectedDrawing.Points.ToList(); 
            EditDrawingMode();
        }
    }

    static void EditDrawingMode()
    {
        int drawingWidth = Console.WindowWidth - 4;
        int drawingHeight = Console.WindowHeight - 4;
        int cursorX = Console.WindowWidth / 2;
        int cursorY = Console.WindowHeight / 2;

        DrawDrawingBorder(drawingWidth, drawingHeight);

        ConsoleKeyInfo input;

        while (true)
        {
            Console.SetCursorPosition(cursorX, cursorY);
            Console.ForegroundColor = activeColor;
            Console.Write(drawingChar);
            Console.ResetColor();

            input = Console.ReadKey(true);

            // Handle cursor movement
            if (input.Key == ConsoleKey.LeftArrow && cursorX > 2) cursorX--;
            else if (input.Key == ConsoleKey.RightArrow && cursorX < drawingWidth + 1) cursorX++;
            else if (input.Key == ConsoleKey.UpArrow && cursorY > 2) cursorY--;
            else if (input.Key == ConsoleKey.DownArrow && cursorY < drawingHeight + 1) cursorY++;

            else if (input.Key == ConsoleKey.Spacebar)
            {
                var point = new Point { X = cursorX, Y = cursorY, Character = drawingChar, Color = activeColor };
                drawingPoints.Add(point);
            }
            else if (input.Key == ConsoleKey.Enter)
            {
                Console.Clear();
                Console.WriteLine("Enter a new name for your edited drawing:");
                string newName = Console.ReadLine();
                SaveDrawing(newName);
                Console.WriteLine($"Edited drawing saved as '{newName}'.");
                Console.WriteLine("Press any key to return to the menu...");
                Console.ReadKey();
                break;
            }
            else if (input.Key == ConsoleKey.Escape)
            {
                break;
            }
        }
    }

    static void DeleteDrawing()
    {
        using (var dbContext = new DrawingDbContext())
        {
            var drawings = dbContext.Drawings.ToList();

            if (drawings.Count == 0)
            {
                Console.Clear();
                Console.WriteLine("No drawings available to delete.");
                Console.WriteLine("Press any key to return to the menu...");
                Console.ReadKey();
                return;
            }

            Console.Clear();
            Console.WriteLine("Select a drawing to delete:");

            for (int i = 0; i < drawings.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {drawings[i].Name}");
            }

            int drawingIndex = -1;
            while (drawingIndex < 0 || drawingIndex >= drawings.Count)
            {
                Console.Write("Enter the number of the drawing to delete: ");
                if (int.TryParse(Console.ReadLine(), out drawingIndex))
                {
                    drawingIndex--;
                }
            }

            var selectedDrawing = drawings[drawingIndex];
            dbContext.Drawings.Remove(selectedDrawing);
            dbContext.SaveChanges();

            Console.WriteLine($"Drawing '{selectedDrawing.Name}' deleted.");
            Console.WriteLine("Press any key to return to the menu...");
            Console.ReadKey();
        }
    }

    static void ViewAllDrawings()
    {
        using (var dbContext = new DrawingDbContext())
        {
            var drawings = dbContext.Drawings.ToList();

            if (drawings.Count == 0)
            {
                Console.Clear();
                Console.WriteLine("No saved drawings to display.");
                Console.WriteLine("Press any key to return to the menu...");
                Console.ReadKey();
                return;
            }

            Console.Clear();
            Console.WriteLine("All Saved Drawings:");

            foreach (var drawing in drawings)
            {
                Console.WriteLine(drawing.Name);
            }

            Console.WriteLine("Press any key to return to the menu...");
            Console.ReadKey();
        }
    }

    static void DrawDrawingBorder(int width, int height)
    {
        Console.ForegroundColor = ConsoleColor.White;

        Console.SetCursorPosition(2, 2);
        Console.WriteLine("╔" + new string('═', width) + "╗");

        for (int i = 0; i < height; i++)
        {
            Console.SetCursorPosition(2, 3 + i);
            Console.Write("║");
            Console.SetCursorPosition(3 + width, 3 + i);
            Console.Write("║");
        }

        Console.SetCursorPosition(2, 3 + height);
        Console.WriteLine("╚" + new string('═', width) + "╝");

        Console.ResetColor();
    }
}

public class Drawing
{
    public int Id { get; set; }
    public string Name { get; set; }
    public List<Point> Points { get; set; } = new List<Point>();
}

public class Point
{
    public int Id { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public char Character { get; set; }
    public ConsoleColor Color { get; set; }
}

public class DrawingDbContext : DbContext
{
    public DbSet<Drawing> Drawings { get; set; }
    public DbSet<Point> Points { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=drawingapp.db");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Drawing>()
            .HasMany(d => d.Points)
            .WithOne()
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Point>()
            .HasKey(p => p.Id);
    }
}
