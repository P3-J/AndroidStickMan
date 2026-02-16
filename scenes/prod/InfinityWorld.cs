using Godot;
using System;
using System.Collections.Generic;

public partial class InfinityWorld : Node3D
{

    [Export] PackedScene Segment_1;
    [Export] PackedScene Segment_2;
    [Export] Node3D segmentParent;
    [Export] Player player;

    List<PackedScene> segments;
    List<SegmentScript> instancedSegments;
    Random rand;

    float segmentTotalLength = 0; // Z

    // 2400/150 = 16 // 32 updated
    // 4.8 ratio


    public override void _Ready()
    {
        base._Ready();
        segments = [
            Segment_1,
            Segment_2
        ];
        instancedSegments = [];

        rand = new Random();

        GenerateSegement(1);
        GenerateSegement(1);

    }


    private void TryLoadNewSegments()
    {
        // free some first
        float playerZ = player.GlobalPosition.Z;
        int clearCount = 0;
        for (int i = instancedSegments.Count - 1; i >= 0; i--)
        {
            SegmentScript seg = instancedSegments[i];
            if (playerZ > seg.endingPosZ)
            {
                clearCount++;
                instancedSegments.RemoveAt(i);
                seg.CallDeferred("queue_free");
            }
        }
        // then generate

        for (int i = 0; i < clearCount; i++)
        {
            GenerateSegement(1);
        }

        GD.Print("Seg count> ", instancedSegments.Count);
        GD.Print("cleared> ", clearCount);


    }

    public void _on_timer_timeout()
    {
        TryLoadNewSegments();
    }


    private void GenerateSegement(int Amount)
    {

        GD.Randomize();
        int randomPickValue = rand.Next(segments.Count);

        PackedScene Segment = segments[randomPickValue];

        // Create Segment

        SegmentScript segmentInstance = Segment.Instantiate<SegmentScript>();
        segmentParent.AddChild(segmentInstance);

        //segmentInstance.GlobalPosition;

        segmentInstance.GlobalPosition = new Vector3(0, 0, segmentTotalLength);

        float endPos = segmentInstance.GetLength() / 32;

        segmentTotalLength += endPos;
        segmentInstance.endingPosZ = segmentTotalLength;

        instancedSegments.Add(segmentInstance);

    }




}
