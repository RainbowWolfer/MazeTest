using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

namespace MazeTest {
	public sealed partial class MainPage: Page {
		public static MainPage Instance;

		public int row;
		public int col;

		public bool finishGenerating;

		public MainPage() {
			Instance = this;
			this.InitializeComponent();
		}
		private async void Page_Loaded(object sender, RoutedEventArgs e) {
			await Task.Delay(1);
			CreateNodes(4, 4);
		}
		public static Grid _testGrid => Instance.testGrid;
		public void SwitchGrid() {
			mainGrid.Visibility = mainGrid.Visibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
			testGrid.Visibility = testGrid.Visibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
		}

		public List<SingleGrid> grids = new List<SingleGrid>();
		public void CreateMaze() {
			for(int x = 0; x < row; x++) {
				mainGrid.RowDefinitions.Add(new RowDefinition());
			}
			for(int y = 0; y < col; y++) {
				mainGrid.ColumnDefinitions.Add(new ColumnDefinition());
			}
			for(int x = 0; x < row; x++) {
				for(int y = 0; y < col; y++) {
					SingleGrid grid = new SingleGrid(GetNode(x, y));
					grids.Add(grid);
					Grid.SetRow(grid, y);
					Grid.SetColumn(grid, x);
					mainGrid.Children.Add(grid);
				}
			}
		}
		public void UpdateMaze() {
			foreach(SingleGrid item in grids) {
				item.Initialize();
			}
		}
		public List<Node> nodes;
		public List<Node> unvisited;
		public async void CreateNodes(int row, int col) {
			this.row = row;
			this.col = col;
			unvisited = new List<Node>();
			nodes = new List<Node>();
			for(int i = 0; i < row; i++) {
				for(int j = 0; j < col; j++) {
					Node n = new Node(i, j) { onWay = false };
					nodes.Add(n);
					unvisited.Add(n);
				}
			}
			CreateMaze();
			Node startNode = nodes[0];
			startNode.previous = null;
			unvisited.Remove(nodes[0]);

			Node next = RandomAvailableNextNode(startNode);
			next.previous = startNode;
			SetDirection(startNode, next);
			while(next != null) {
				if(unvisited.Contains(next)) {
					unvisited.Remove(next);
				}
				Node tmp = next;
				next = RandomAvailableNextNode(next);
				if(next != null) {
					SetDirection(tmp, next);
					next.previous = tmp;
				} else {
					UpdateMaze();
					if(unvisited.Count <= 0) {
						break;
					}
					await Task.Delay(1);
					Node backTrace = BackTrace(tmp);
					Debug.WriteLine(backTrace.pos.ToString());
					next = backTrace;
				}
			}
			await Task.Delay(10);
			finishGenerating = true;
			var pf = new Pathfinding(nodes);
			//pf.Log();
			List<PFNode> path = pf.FindPath(new Vector2(0, 0), new Vector2(row * 2 - 2, col * 2 - 2));
			List<Node> convertedPath = pf.ConvertToOriginalPath(path);
			pf.DrawInGrid(convertedPath);

			foreach(Node item in nodes) {
				if(convertedPath.Contains(item)) {
					grids.Find((g) => g.node == item).SetFill(Colors.Green);
				}
			}
		}

		public Node BackTrace(Node node) {
			Node next = RandomAvailableNextNode(node.previous);
			if(next != null) {
				return node.previous;
			}
			while(next == null) {
				node = node.previous;
				next = RandomAvailableNextNode(node);
			}
			return node;
		}

		public void CreatePath(Node endNode) {
			while(endNode.previous != null) {
				SetDirection(endNode.previous, endNode);
				endNode = endNode.previous;
			}
		}

		public void SetDirection(Node from, Node to) {
			Vector2 d = to.pos - from.pos;
			if(d == Vector2.Up) {
				from.d.up = true;
				to.d.down = true; ;
			} else if(d == Vector2.Left) {
				from.d.left = true;
				to.d.right = true;
			} else if(d == Vector2.Down) {
				from.d.down = true;
				to.d.up = true;
			} else if(d == Vector2.Right) {
				from.d.right = true;
				to.d.left = true;
			} else {
				throw new Exception("Can not set direction");
			}
		}

		public void PrintPath(Node endNode) {
			foreach(Node item in nodes) {
				item.onWay = false;
			}
			while(endNode.previous != null) {
				//Debug.WriteLine(endNode.pos);
				endNode.onWay = true;
				endNode = endNode.previous;
			}
			endNode.onWay = true;
			UpdateMaze();
		}
		public Node GetNode(Vector2 v) {
			if(v.x >= row || v.x < 0 || v.y >= col || v.y < 0) {
				return null;
			}
			foreach(Node item in nodes) {
				if(item.pos == v) {
					return item;
				}
			}
			return null;
		}
		public Node GetNode(int x, int y) {
			return GetNode(new Vector2(x, y));
		}
		public Node RandomAvailableNextNode(Node node) {
			List<Node> nexts = new List<Node>();
			foreach(Vector2 item in Vector2.AllDirections) {
				Node current = GetNode(item + node.pos);
				if(current == null) {
					continue;
				}
				if(unvisited.Contains(current)) {
					nexts.Add(current);
				}
			}
			if(nexts.Count == 0) {
				return null;
			}
			int r = new Random().Next(0, nexts.Count);
			return nexts[r];
		}
		public Vector2 RandomDirection() {
			int r = new Random().Next(0, 100);
			if(r < 25) {
				return Vector2.Up;
			} else if(r < 50) {
				return Vector2.Left;
			} else if(r < 75) {
				return Vector2.Down;
			} else {
				return Vector2.Right;
			}
		}
	}
	public class Pathfinding {
		private readonly List<Node> originalNode;
		public const int MOVE_DIAGONAL_COST = 14;
		public const int MOVE_STRAIGHT_COST = 10;

		private int row => MainPage.Instance.row * 2 - 1;
		private int col => MainPage.Instance.col * 2 - 1;

		public Dictionary<Vector2, PFNode> nodes;
		public Pathfinding(List<Node> ns) {
			originalNode = ns;
			nodes = new Dictionary<Vector2, PFNode>();
			foreach(Node n in ns) {
				nodes[n.pos * 2] = new PFNode(n.pos * 2) { walkable = true };
				for(int i = 0; i < 4; i++) {
					Vector2 v = n.pos * 2 + Vector2.AllDirections[i];
					if(IsWithinBorder(v)) {
						if(i == 0 && n.d.left) {
							nodes[v] = new PFNode(v) { walkable = true };
						} else if(i == 1 && n.d.up) {
							nodes[v] = new PFNode(v) { walkable = true };
						} else if(i == 2 && n.d.right) {
							nodes[v] = new PFNode(v) { walkable = true };
						} else if(i == 3 && n.d.down) {
							nodes[v] = new PFNode(v) { walkable = true };
						}
					}
				}
			}
			for(int x = 0; x < row; x++) {
				for(int y = 0; y < col; y++) {
					if(!nodes.ContainsKey(new Vector2(x, y))) {
						nodes.Add(new Vector2(x, y), new PFNode(x, y) { walkable = false });
					}
				}
			}
		}

		private static List<PFNode> openList;
		private static List<PFNode> closeList;

		public List<Node> ConvertToOriginalPath(List<PFNode> nodes) {
			List<Node> result = new List<Node>();
			foreach(PFNode pf in nodes) {
				foreach(Node n in originalNode) {
					if(pf.pos / 2 == n.pos) {
						if(pf.pos.x % 2 != 1 && pf.pos.y % 2 != 1) {
							result.Add(n);
						}
						break;
					}
				}
			}
			return result;
		}

		public List<PFNode> FindPath(Vector2 start, Vector2 end) {
			PFNode startNode = nodes[start];
			PFNode endNode = nodes[end];

			openList = new List<PFNode>() { startNode };
			closeList = new List<PFNode>();

			foreach(PFNode node in nodes.Values) {
				if(!node.walkable) {
					closeList.Add(node);
				}
			}

			for(int x = 0; x < row; x++) {
				for(int y = 0; y < col; y++) {
					PFNode node = nodes[new Vector2(x, y)];
					node.gCost = int.MaxValue;
					node.previous = null;
				}
			}

			startNode.gCost = 0;
			startNode.hCost = CalculateDistanceCost(startNode, endNode);

			while(openList.Count > 0) {
				PFNode currentNode = GetLowestFCostNode(openList);
				if(currentNode == endNode) {
					return CalculatePath(endNode);
				}
				openList.Remove(currentNode);
				closeList.Add(currentNode);

				List<PFNode> ns = GetNeighbours(currentNode);
				foreach(PFNode node in ns) {
					if(closeList.Contains(node)) {
						continue;
					}
					int distanceCost = CalculateDistanceCost(currentNode, node);
					int tentativeGCost = currentNode.gCost + distanceCost;
					if(tentativeGCost < node.gCost) {
						node.previous = currentNode;
						node.gCost = tentativeGCost;
						node.hCost = CalculateDistanceCost(node, endNode);

						if(!openList.Contains(node)) {
							openList.Add(node);
						}
					}
				}
			}
			throw new Exception("No Path Found");
		}
		public List<PFNode> GetNeighbours(PFNode current) {
			List<PFNode> result = new List<PFNode>();
			if(current.pos.x >= 1) {
				result.Add(nodes[current.pos + Vector2.Left]);
			}
			if(current.pos.x < row - 1) {
				result.Add(nodes[current.pos + Vector2.Right]);
			}
			if(current.pos.y >= 1) {
				result.Add(nodes[current.pos + Vector2.Up]);
			}
			if(current.pos.y < col - 1) {
				result.Add(nodes[current.pos + Vector2.Down]);
			}
			return result;
		}
		public List<PFNode> CalculatePath(PFNode endNode) {
			List<PFNode> path = new List<PFNode>() { endNode };
			PFNode current = endNode;
			while(current.previous != null) {
				path.Add(current.previous);
				current = current.previous;
			}
			path.Reverse();
			return path;
		}
		public PFNode GetLowestFCostNode(List<PFNode> ns) {
			PFNode lowest = ns[0];
			foreach(PFNode item in ns) {
				if(item.fCost < lowest.fCost) {
					lowest = item;
				}
			}
			return lowest;
		}
		public int CalculateDistanceCost(PFNode a, PFNode b) {
			int xDistance = Math.Abs(a.pos.x - b.pos.y);
			int yDistance = Math.Abs(a.pos.y - b.pos.y);
			int remaining = Math.Abs(xDistance - yDistance);
			return MOVE_DIAGONAL_COST * Math.Min(xDistance, yDistance) + MOVE_STRAIGHT_COST * remaining;
		}

		public bool IsWithinBorder(Vector2 v) {
			return v.x >= 0 && v.y >= 0 && v.x < row && v.y < col;
		}
		public void Log() {
			for(int x = 0; x < row; x++) {
				string line = "";
				for(int y = 0; y < col; y++) {
					//Node n = nodes[new Vector2(x, y)];
					bool b = nodes.ContainsKey(new Vector2(x, y));
					line += x + ":" + y + (b ? "YES" : "no") + "\t";
				}
				Debug.WriteLine(line + "\n");
			}
		}
		public void DrawInGrid(List<Node> path) {
			for(int x = 0; x < row; x++) {
				MainPage._testGrid.RowDefinitions.Add(new RowDefinition());
			}
			for(int y = 0; y < col; y++) {
				MainPage._testGrid.ColumnDefinitions.Add(new ColumnDefinition());
			}
			for(int x = 0; x < row; x++) {
				for(int y = 0; y < col; y++) {
					string str = x + ":" + y + "\n";
					str += nodes[new Vector2(x, y)].walkable ? "YES\n" : "no\n";
					SingleGrid sg = new SingleGrid(null, str);
					bool inPath = false;
					foreach(Node n in path) {
						if(n.pos * 2 == new Vector2(x, y)) {
							inPath = true;
							break;
						}
					}
					sg.SetFill(nodes[new Vector2(x, y)].walkable ? inPath ? Colors.Blue : Colors.Transparent : Colors.Red);
					MainPage._testGrid.Children.Add(sg);
					Grid.SetRow(sg, y);
					Grid.SetColumn(sg, x);
				}
			}
		}
	}
	public class PFNode {
		public Vector2 pos;
		public bool walkable;
		public int gCost;
		public int hCost;
		public int fCost => gCost + hCost;
		public PFNode previous;
		public PFNode(Vector2 pos) {
			this.pos = pos;
		}
		public PFNode(int x, int y) {
			this.pos = new Vector2(x, y);
		}
	}

	public class Node {
		public Vector2 pos;
		public Direction d;

		public bool onWay;

		public Node previous;
		public Node(int x, int y) {
			this.pos = new Vector2(x, y);
			d = Direction.ALLFALSE;
		}
		public Node(Vector2 pos) {
			this.pos = pos;
			d = Direction.ALLFALSE;
		}
		public override string ToString() {
			return "Node " + pos;
		}
	}
	public struct Direction {
		public bool up;
		public bool left;
		public bool down;
		public bool right;

		public static Direction ALLFALSE = new Direction(false, false, false, false);
		public static Direction ALLTRUE = new Direction(true, true, true, true);
		public Direction(bool up, bool left, bool down, bool right) {
			this.up = up;
			this.left = left;
			this.down = down;
			this.right = right;
		}
	}
	public struct Vector2 {
		public int x;
		public int y;

		public static Vector2 Up => new Vector2(0, -1);
		public static Vector2 Right => new Vector2(1, 0);
		public static Vector2 Down => new Vector2(0, 1);
		public static Vector2 Left => new Vector2(-1, 0);
		public static Vector2 Zero => new Vector2(0, 0);
		public static Vector2[] AllDirections => new Vector2[] { Left, Up, Right, Down };

		public Vector2(int x, int y) {
			this.x = x;
			this.y = y;
		}

		public static Vector2 operator +(Vector2 a, Vector2 b) {
			return new Vector2(a.x + b.x, a.y + b.y);
		}
		public static Vector2 operator -(Vector2 a, Vector2 b) {
			return new Vector2(a.x - b.x, a.y - b.y);
		}
		public static Vector2 operator *(Vector2 a, int b) {
			return new Vector2(a.x * b, a.y * b);
		}
		public static Vector2 operator /(Vector2 a, int b) {
			return new Vector2(a.x / b, a.y / b);
		}
		public static bool operator ==(Vector2 a, Vector2 b) {
			return a.x == b.x && a.y == b.y;
		}
		public static bool operator !=(Vector2 a, Vector2 b) {
			return !(a == b);
		}
		public override string ToString() {
			return x + " : " + y;
		}
	}
}
