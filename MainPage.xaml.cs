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
		//public Dictionary<Vector2, SingleGrid> cells;

		public int row;
		public int col;

		public MainPage() {
			Instance = this;
			this.InitializeComponent();
			//CreateMaze(10, 10, Vector2.Zero);
			//CreateNodes(10, 10);
			//Node n1 = new Node(1, 1);
			//Node n2 = new Node(1, 2);

			//SetDirection(n1, n2);
		}
		private async void Page_Loaded(object sender, RoutedEventArgs e) {
			//Debug.WriteLine("Begin");
			await Task.Delay(100);
			CreateNodes(50, 50);
			//Debug.WriteLine("End");
		}

		//private int maxRow;
		//private int maxColume;
		//private List<Vector2> visited;
		//private List<Vector2> unvisited;
		public List<SingleGrid> grids = new List<SingleGrid>();
		public void CreateMaze() {
			//cells = new Dictionary<Vector2, SingleGrid>();
			//visited = new List<Vector2>();
			//unvisited = new List<Vector2>();
			//maxRow = row;
			//maxColume = col;
			for(int x = 0; x < row; x++) {
				mainGrid.RowDefinitions.Add(new RowDefinition());
			}
			for(int y = 0; y < col; y++) {
				mainGrid.ColumnDefinitions.Add(new ColumnDefinition());
			}
			for(int x = 0; x < row; x++) {
				for(int y = 0; y < col; y++) {
					//Debug.WriteLine(x + ":" + y);
					SingleGrid grid = new SingleGrid(GetNode(x, y));
					//grid.SetDirections(App.GetRandomBoolean(), App.GetRandomBoolean(), App.GetRandomBoolean(), App.GetRandomBoolean());
					//cells.Add(new Vector2(x, y), grid);
					grids.Add(grid);
					Grid.SetRow(grid, y);
					Grid.SetColumn(grid, x);
					mainGrid.Children.Add(grid);
				}
			}


			//cells.ContainsKey()

		}
		public void UpdateMaze() {
			foreach(SingleGrid item in grids) {
				item.Initialize();
			}
		}
		public List<Node> nodes;
		//public List<Node> visited;
		public List<Node> unvisited;
		public async void CreateNodes(int row, int col) {
			this.row = row;
			this.col = col;
			//visited = new List<Node>();
			unvisited = new List<Node>();
			nodes = new List<Node>();
			for(int i = 0; i < row; i++) {
				for(int j = 0; j < col; j++) {
					Node n = new Node(i, j);
					nodes.Add(n);
					unvisited.Add(n);
				}
			}
			CreateMaze();
			//unvisited = nodes;
			Node startNode = nodes[0];
			startNode.previous = null;
			unvisited.Remove(nodes[0]);

			Node next = RandomAvailableNextNode(startNode);
			next.previous = startNode;
			SetDirection(startNode, next);
			while(next != null) {
				if(unvisited.Contains(next)) {
					unvisited.Remove(next);
				} else {

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
					await Task.Delay(50);
					//PrintPath(tmp);
					Node backTrace = BackTrace(tmp);
					Debug.WriteLine(backTrace.pos.ToString());


					next = backTrace;
					//CreatePath(tmp);
					//break;
				}
			}
			//CreateMaze();
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

		private void PrintPath(Node endNode) {
			while(endNode.previous != null) {
				//Debug.WriteLine(endNode.pos);
				endNode = endNode.previous;
			}
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
		//public void CreatePath(Vector2 p1, Directions d) {
		//	switch(d) {
		//		case Directions.Up:
		//			cells[p1].up = true;
		//			cells[p1 + Vector2.Up].down = true;
		//			break;
		//		case Directions.Left:
		//			cells[p1].left = true;
		//			cells[p1 + Vector2.Left].right = true;
		//			break;
		//		case Directions.Down:
		//			cells[p1].down = true;
		//			cells[p1 + Vector2.Down].up = true;
		//			break;
		//		case Directions.Right:
		//			cells[p1].right = true;
		//			cells[p1 + Vector2.Right].left = true;
		//			break;
		//		default:
		//			throw new Exception("???");
		//	}
		//}
		public class Node {
			public Vector2 pos;
			public Direction d;
			public Node previous;
			public Node(int x, int y) {
				this.pos = new Vector2(x, y);
				d.up = false;
				d.left = false;
				d.down = false;
				d.right = false;
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

		//public Vector2[] GetSurrounding(Vector2 v) {
		//	List<Vector2> result = new List<Vector2>();
		//	if(v.x + 1 < maxRow) {
		//		result.Add(v + Vector2.Right);
		//	}
		//	if(v.x - 1 >= 0) {
		//		result.Add(v + Vector2.Left);
		//	}
		//	if(v.y - 1 >= 0) {
		//		result.Add(v + Vector2.Down);
		//	}
		//	if(v.y + 1 < maxColume) {
		//		result.Add(v + Vector2.Up);
		//	}
		//	return result.ToArray();
		//}
		/*
		public bool CheckNext(Vector2 v) {
			foreach(Vector2 item in Vector2.AllDirections) {
				if(!visited.Contains(v + item)) {
					return true;
				}
			}
			return false;
		}
		*/
	}
	public struct Vector2 {
		public int x;
		public int y;

		public static Vector2 Up => new Vector2(0, -1);
		public static Vector2 Right => new Vector2(1, 0);
		public static Vector2 Down => new Vector2(0, 1);
		public static Vector2 Left => new Vector2(-1, 0);
		public static Vector2 Zero => new Vector2(0, 0);
		public static Vector2[] AllDirections => new Vector2[] { Up, Down, Left, Right };

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
