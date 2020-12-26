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
			pf.Log();
			pf.DrawInGrid();
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
		private Node[] originNodes;

		private int row => MainPage.Instance.row * 2 - 1;
		private int col => MainPage.Instance.col * 2 - 1;

		public Dictionary<Vector2, Node> nodes;
		public Pathfinding(List<Node> ns) {
			originNodes = ns.ToArray();
			nodes = new Dictionary<Vector2, Node>();

			foreach(Node n in ns) {
				//nodes.Add(n.pos, n);
				n.walkable = true;
				nodes[n.pos * 2] = n;
				for(int i = 0; i < 4; i++) {
					Vector2 v = n.pos * 2 + Vector2.AllDirections[i];
					if(IsWithinBorder(v)) {
						if(i == 0 && n.d.left) {
							nodes[v] = new Node(v) { walkable = true };
						} else if(i == 1 && n.d.up) {
							nodes[v] = new Node(v) { walkable = true };
						} else if(i == 2 && n.d.right) {
							nodes[v] = new Node(v) { walkable = true };
						} else if(i == 3 && n.d.down) {
							nodes[v] = new Node(v) { walkable = true };
						}
					}
				}
			}
			for(int x = 0; x < row; x++) {
				for(int y = 0; y < col; y++) {
					if(!nodes.ContainsKey(new Vector2(x, y))) {
						nodes.Add(new Vector2(x, y), new Node(x, y) { walkable = false });
					}
				}
			}
		}
		public List<Node> FindPath(Vector2 start, Vector2 end) {

			return new List<Node>();
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
		public void DrawInGrid() {
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
					SingleGrid sg = new SingleGrid(nodes[new Vector2(x, y)], str);
					sg.SetFill(nodes[new Vector2(x, y)].walkable ? Colors.Transparent : Colors.Red);
					MainPage._testGrid.Children.Add(sg);
					Grid.SetRow(sg, y);
					Grid.SetColumn(sg, x);
				}
			}
		}
	}
	public class Node {
		public Vector2 pos;
		public Direction d;

		public bool onWay;

		public bool walkable;

		public int gCost;
		public int hCost;
		public int fCost => gCost + hCost;

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
