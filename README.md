# Multithreaded3DAStarWithHeap
A multithreaded A* pathfinding, working for 2 or 3 Dimensions and for big nodegraphs

<h1>A Star Pathfinder</h1>
<p>This is a custom A* pathfinding algorithm that is suitable to be used for 2-Dimensional and 3-Dimensional pathfinding
(flying units is a strategy game, multilayered levels or flying games).</p>
<b>
<p>The A star sort will make use of multiple threads to allow many parallel units for path evaluation, while not blocking the
main game thread. The implemented open list makes use of a custom Heap and is therefore suitable for sorting huge graphs, 
since it's complexity is O(nlogn), instead of O(n), which obviously scales better with bigger graphs</p></b>

<p>NOTE: this does not work properly with Unity 2018.4, due to a bug in Unity not collecting threads properly. 
  2019.1.10f works properly since the issue got fixed by Unity in the meantime.</p>
