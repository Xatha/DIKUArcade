using BenchmarkDotNet.Attributes;
using DIKUArcade.Entities;
using DIKUArcade.Graphics;
using DIKUArcade.Math;

namespace DIKUArcadeBenchmarks.EntityBenchmarks;

[MemoryDiagnoser(true)]
public class EntityContainerBenchmark
{
    private static Random random;
    
    [Params(100, 10_000, 100_000)]
    public int EntityCount;

    private EntityContainer container;
    
    private EntityContainerBuffered containerBuffered;
    
    private int count;

    [GlobalSetup]
    public void Setup()
    {
        count = 0;
        random = new Random(69);
        container = SetupContainer(EntityCount); 
        containerBuffered = SetupBufferedContainer(EntityCount);
    }
    
    
    EntityContainer SetupContainer(int n) 
    {
        EntityContainer bcontainer = new EntityContainer();

        for (int i = 0; i < n; i++)
        {
            StationaryShape shape = new StationaryShape(new Vec2F(0.0f, 0.0f), new Vec2F(0.0f, 0.0f));
            IBaseImage image = new MockImage();
            bcontainer.AddStationaryEntity(shape, image);
        }

        return bcontainer;
    }
    
    EntityContainerBuffered SetupBufferedContainer(int n)
    {
        EntityContainerBuffered bcontainer = new EntityContainerBuffered(2);

        for (int i = 0; i < n; i++)
        {
            StationaryShape shape = new StationaryShape(new Vec2F(0.0f, 0.0f), new Vec2F(0.0f, 0.0f));
            IBaseImage image = new MockImage();
            bcontainer.AddStationaryEntity(shape, image);
        }
        return bcontainer;
    }

    
    [Benchmark]
    public void Normal_Iterate()
    {
        container.Iterate(e =>
        {
            var r = Random.Shared.Next(100);
            if (r > 50)
            {
                count += r;
            }
        });
    }

   [Benchmark]
    public void Buffered_Mutating_Sequential_Iterate()
    {
        containerBuffered.Iterate(e =>
        {
            var r = Random.Shared.Next(100);
            if (r > 50)
            {
                count += r;
            }
        });
    }
    
    [Benchmark]
    public void Buffered_Immutable_Parallel_Iterate()
    {
        containerBuffered.ImmutableIterate(e =>
        {
            var r = Random.Shared.Next(100);
            if (r > 50)
            {
                count += r;
            }
        });
    }

    [Benchmark]
    public void Normal_ForEach()
    {
        foreach (Entity entity in container)
        {
            var r = Random.Shared.Next(100);
            if (r > 50)
            {
                count += r;
            }
        }
        
    }
    
    [Benchmark]
    public void Buffered_ForEach()
    {
        foreach (Entity entity in containerBuffered)
        {
            var r = Random.Shared.Next(100);
            if (r > 50)
            {
                count += r;
            }
        }
    }


    //[Benchmark]
    public void SetupContainer()
    {
        SetupContainer(EntityCount);
    }

    /*
    [Benchmark]
    public void ForEach()
    {
        foreach (Entity e in container)
        {
            if (random.Next(100) > 50)
            {
                e.DeleteEntity();
            }
        }
    }
    
    [Benchmark]
    public void For()
    {
        for (int i = 0; i < container.entities.Count; i++)
        {
            if (random.Next(100) > 50)
            {
                container.entities[i].DeleteEntity();
            }
        }
    }*/
}