using EvoPrototype.Core;
using EvoPrototype.Evolution;
using Godot;

namespace EvolutionTwo.Presentation;

/// <summary>Presentation-only observation. Never modifies agents or their decisions.</summary>
public sealed class ArenaAttention
{
    private Random _random = new(3701);
    private double _nextMeasure, _nextSharedChoice;
    private readonly double[] _nextChoice = new double[2];
    private readonly double[] _nextBlink = {2.1,3.4};
    private readonly double[] _blinkUntil = new double[2];
    private readonly Agent?[] _tracked = new Agent?[2];
    private readonly bool[] _shared = {true,true};
    private readonly bool[] _lowerChin = new bool[2];
    private Vector2 _sharedPoint = new(.5f,.5f);
    private readonly Vector2[] _points = {new(.5f,.5f),new(.5f,.5f)};
    private readonly List<Agent> _visible = new(480);
    public float Concentration { get; private set; }
    public Vector2 Focus { get; private set; } = new(.5f,.5f);
    public Vector2 Point(int actor) => _points[actor];
    public bool Blinking(int actor, double now) => now < _blinkUntil[actor];
    public bool TrackingAgent => _tracked.Any(a=>a is not null);

    public void Reset()
    {
        _random=new(3701); _nextMeasure=_nextSharedChoice=0; Concentration=0;
        _visible.Clear(); Array.Clear(_tracked); Array.Clear(_nextChoice);
        Array.Clear(_lowerChin);
        Array.Fill(_shared,true); Array.Fill(_points,new Vector2(.5f,.5f));
        _nextBlink[0]=2.1;_nextBlink[1]=3.4;Array.Clear(_blinkUntil);
    }

    public void Update(double now, double delta, LevelOneSession session)
    {
        if(now>=_nextMeasure)
        {
            _nextMeasure=now+.25;_visible.Clear();
            foreach(var team in new[]{session,session.Opponent!})
                foreach(var a in team.Simulation.WorldState.Agents)
                    if(team.Cycle.Phase!=EvolutionPhase.Selection || team.Survivors.Contains(a)) _visible.Add(a);
            var result=MeasureCluster(_visible.Select(a=>a.Position/session.ArenaSize).ToArray());
            Focus=result.Point;Concentration=result.Share;
        }
        if(now>=_nextSharedChoice)
        {
            _nextSharedChoice=now+2.3+_random.NextDouble()*3.8;
            _sharedPoint=Concentration>=.35f?Focus:RandomPoint(session.ArenaSize);
        }
        if(Concentration>=.9f) _sharedPoint=Focus;
        for(var actor=0;actor<2;actor++)
        {
            if(now>=_nextBlink[actor])
            {
                _blinkUntil[actor]=now+.10;_nextBlink[actor]=now+2.4+_random.NextDouble()*4.9;
            }
            if(now>=_nextChoice[actor] || (_tracked[actor] is not null && !_visible.Contains(_tracked[actor]!)))
            {
                _nextChoice[actor]=now+1.1+_random.NextDouble()*3.5;
                _lowerChin[actor]=_random.NextDouble()<.25;
                _shared[actor]=_random.NextDouble()<(Concentration>=.9f?.95:.82);
                _tracked[actor]=!_shared[actor] && _visible.Count>0?_visible[_random.Next(_visible.Count)]:null;
            }
            var target=_shared[actor]?_sharedPoint:_tracked[actor]?.Position/session.ArenaSize ?? _sharedPoint;
            // Each actor catches up at a different rate; no simultaneous snapping.
            _points[actor]=_points[actor].Lerp(target,1-MathF.Exp(-(float)delta*(actor==0?5.4f:3.8f)));
        }
    }
    private Vector2 RandomPoint(Vector2 size) => _visible.Count==0?new(.5f,.5f):_visible[_random.Next(_visible.Count)].Position/size;

    public static (Vector2 Point,float Share) MeasureCluster(IReadOnlyList<Vector2> positions)
    {
        if(positions.Count==0)return(new(.5f,.5f),0);
        // Overlapping windows avoid losing a dense cluster on a histogram boundary.
        var best=0;var center=new Vector2(.5f,.5f);
        for(var y=0;y<=8;y++) for(var x=0;x<=10;x++)
        {
            var candidate=new Vector2(x/10f,y/8f);var count=0;var sum=Vector2.Zero;
            foreach(var p in positions)
                if(MathF.Abs(p.X-candidate.X)<=.15f && MathF.Abs(p.Y-candidate.Y)<=.19f){count++;sum+=p;}
            if(count>best){best=count;center=sum/count;}
        }
        return(center,best/(float)positions.Count);
    }

    public int Frame(int actor, bool speaking, double speechAge)
    {
        var p=_points[actor];
        var frame=p.X<.38f?0:p.X>.62f?2:4;
        if(speaking && (int)(speechAge*7)%3!=0) return frame+1;
        if((p.Y>.7f || _lowerChin[actor]) && frame!=4)return frame==0?6:7;
        return frame;
    }
}
