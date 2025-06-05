using System.Numerics;
using Ardalis.GuardClauses;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using JetBrains.Annotations;
using SSuite.PhysicsSimulator.Core;
using SSuite.PhysicsSimulator.Core.PhysicalObjects;
using SSuite.PhysicsSimulator.Core.ValueObjects;
// ReSharper disable UnusedParameter.Local

namespace SSuite.PhysicsSimulator.UI;

public partial class MainWindow : Window
{
    private Engine? physicsEngine;
    private readonly DispatcherTimer simulationTimer;
    private readonly Dictionary<PhysicalObject, Shape> objectShapes = new();
    private bool isSimulationRunning;
    private readonly Random random = new();

    public MainWindow()
    {
        this.InitializeComponent();

        const string valuePropertyName = "Value";

        this.AirResistanceSlider.PropertyChanged += (sender, @event) =>
        {
            if (@event.Property.Name.Equals(valuePropertyName))
            {
                this.AirResistanceValue.Text = this.AirResistanceSlider.Value.ToString("0.00");
            }
        };

        this.simulationTimer = new DispatcherTimer
        {
            //This is roughly 60 fps
            Interval = TimeSpan.FromMilliseconds(16)
        };

        this.simulationTimer.Tick += this.SimulationTimerTick;

        this.InitializePhysicsEngine();
    }

    private void InitializePhysicsEngine()
    {
        var gravityY = this.GravityY.Value;
        Guard.Against.Null(gravityY);

        var gravity = new Vector2(0, (float)gravityY);
        var airResistance = new AirResistance((float)this.AirResistanceSlider.Value);
        this.physicsEngine = new Engine(gravity, airResistance);
    }

    [UsedImplicitly]
    private void OnStartSimulation(object? sender, RoutedEventArgs e)
    {
        if (this.isSimulationRunning)
        {
            return;
        }

        this.isSimulationRunning = true;
        this.simulationTimer.Start();
        this.StartButton.IsEnabled = false;
        this.PauseButton.IsEnabled = true;

    }

    private void OnPauseSimulation(object? sender, RoutedEventArgs e)
    {
        if (!this.isSimulationRunning)
        {
            return;
        }

        this.isSimulationRunning = false;
        this.simulationTimer.Stop();
        this.StartButton.IsEnabled = true;
        this.PauseButton.IsEnabled = false;
    }

    [UsedImplicitly]
    private void OnResetSimulation(object? sender, RoutedEventArgs e)
    {
        this.OnPauseSimulation(sender, e);
        this.InitializePhysicsEngine();

        this.SimulationCanvas.Children.Clear();
        this.objectShapes.Clear();
    }

    [UsedImplicitly]
    private void OnAddCircle(object? sender, RoutedEventArgs e)
    {
        if (this.physicsEngine is null)
        {
            return;
        }

        var canvasWidth = this.SimulationCanvas.Bounds.Width;
        var canvasHeight = this.SimulationCanvas.Bounds.Height;

        if (canvasWidth <= 0 || canvasHeight <= 0)
        {
            canvasWidth = 800;
            canvasHeight = 600;
        }

        var x = this.random.Next(50, (int)canvasWidth - 50);
        var y = this.random.Next(50, (int)canvasHeight / 3);
        var radius = this.random.Next(10, 40);
        var mass = radius * 0.5f;

        var position = new Vector2(x, y);
        var velocity = new Vector2(this.random.Next(-5, 5), 0);

        var physicalObject = new Circle
        {
            Position = position,
            Velocity = velocity,
            Acceleration = Vector2.Zero,
            Mass = mass,
            Radius = radius,
            Restitution = 0.7f,
            Friction = 0.3f
        };

        this.physicsEngine.AddPhysicalObject(physicalObject);

        var circle = new Ellipse
        {
            Width = radius * 2,
            Height = radius * 2,
            Fill = new SolidColorBrush(Color.FromRgb(
                (byte)this.random.Next(100, 255),
                (byte)this.random.Next(100, 255),
                (byte)this.random.Next(100, 255)
            ))
        };

        Canvas.SetLeft(circle, x - radius);
        Canvas.SetTop(circle, y - radius);

        this.SimulationCanvas.Children.Add(circle);
        this.objectShapes.Add(physicalObject, circle);
    }

    [UsedImplicitly]
    private void OnClearObjects(object? sender, RoutedEventArgs e)
    {
        if (this.physicsEngine is null)
        {
            return;
        }

        foreach (var physicalObject in this.objectShapes.Keys)
        {
            this.physicsEngine.RemovePhysicalObject(physicalObject);
        }

        this.SimulationCanvas.Children.Clear();
        this.objectShapes.Clear();
    }

    private void SimulationTimerTick(object? sender, EventArgs e)
    {
        if (this.physicsEngine is null)
        {
            return;
        }

        this.physicsEngine.UpdatePhysicalObjects(new DeltaTime(0.016f));
        this.CheckBoundaryCollisions();

        foreach (var (physicalObject, shape) in this.objectShapes)
        {
            var radiusValue = (physicalObject as Circle)?.Radius.Value ?? 0;
            Canvas.SetLeft(shape, physicalObject.Position.X - radiusValue);
            Canvas.SetTop(shape, physicalObject.Position.Y - radiusValue);
        }
    }
    private void CheckBoundaryCollisions()
    {
        if (this.physicsEngine is null || this.SimulationCanvas is null)
        {
            return;
        }

        var canvasHeight = this.SimulationCanvas.Bounds.Height;
        var canvasWidth = this.SimulationCanvas.Bounds.Width;

        if (canvasHeight <= 0)
        {
            canvasHeight = 600;
        }

        if (canvasWidth <= 0)
        {
            canvasWidth = 800;
        }


        foreach (var (physicalObject, _) in this.objectShapes)
        {
            var circle = physicalObject as Circle;
            if (circle is null)
            {
                continue;
            }

            var bottomPosition = circle.Position.Y + circle.Radius.Value;

            if (bottomPosition >= canvasHeight)
            {
                circle.Position = circle.Position with { Y = (float) canvasHeight - circle.Radius.Value };

                var velocityX = circle.Velocity.X * 0.9f;
                var velocityY = -circle.Velocity.Y * 0.7f;

                circle.Velocity = new Vector2(velocityX, velocityY);
            }

            var leftPosition = circle.Position.X - circle.Radius.Value;
            if (leftPosition <= 0)
            {
                circle.Position = circle.Position with { X = circle.Radius.Value };
                circle.Velocity = circle.Velocity with { X = -circle.Velocity.X * 0.7f };
            }

            var rightPosition = circle.Position.X + circle.Radius.Value;

            if (rightPosition < canvasWidth)
            {
                return;
            }

            circle.Position = circle.Position with { X = (float)canvasWidth - circle.Radius.Value };
            circle.Velocity = circle.Velocity with { X = -circle.Velocity.X * 0.7f };
        }
    }

}
