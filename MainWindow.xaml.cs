
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace NodeGraph;

/// <summary>
/// 主窗口类，负责管理节点编辑器的核心逻辑。
/// 包括节点的创建、连接、拖拽以及运行操作。
/// </summary>
public partial class MainWindow : Window
{
    /// <summary>
    /// 存储所有节点的列表。
    /// </summary>
    private List<Node> nodes = new List<Node>();

    /// <summary>
    /// 当前正在拖拽的起始连接器。
    /// </summary>
    private Connector startConnector;

    /// <summary>
    /// 表示当前拖拽中的连接线。
    /// </summary>
    private Line connectionLine;

    /// <summary>
    /// 是否正在进行连接拖拽操作。
    /// </summary>
    private bool isDraggingConnection = false;

    /// <summary>
    /// 存储连接器与其对应的视觉元素（椭圆）。
    /// </summary>
    private Dictionary<Connector, Ellipse> connectorVisuals = new Dictionary<Connector, Ellipse>();

    /// <summary>
    /// 记录鼠标最后的位置，用于右键菜单定位。
    /// </summary>
    private Point lastMousePosition;

    /// <summary>
    /// 初始化主窗口，加载组件并设置连接线。
    /// </summary>
    public MainWindow()
    {
        InitializeComponent();
        InitializeConnectionLine();
        nodeCanvas.MouseMove += nodeCanvas_MouseMove;
    }

    /// <summary>
    /// 初始化连接线对象，设置其样式并添加到画布中。
    /// </summary>
    private void InitializeConnectionLine()
    {
        connectionLine = new Line
        {
            Stroke = Brushes.White,
            StrokeThickness = 2,
            Visibility = Visibility.Collapsed
        };
        nodeCanvas.Children.Add(connectionLine);
    }

    /// <summary>
    /// 处理画布上的右键点击事件，显示上下文菜单。
    /// </summary>
    private void nodeCanvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        lastMousePosition = e.GetPosition(nodeCanvas);
        nodeContextMenu.IsOpen = true;
    }

    /// <summary>
    /// 创建一个浮点数节点并将其添加到画布中。
    /// </summary>
    private void CreateFloatNode_Click(object sender, RoutedEventArgs e)
    {
        var node = new FloatNode();
        AddNodeToCanvas(node, lastMousePosition);
    }

    /// <summary>
    /// 创建一个加法节点并将其添加到画布中。
    /// </summary>
    private void CreateAddNode_Click(object sender, RoutedEventArgs e)
    {
        var node = new AddNode();
        AddNodeToCanvas(node, lastMousePosition);
    }

    /// <summary>
    /// 创建一个打印节点并将其添加到画布中。
    /// </summary>
    private void CreatePrintNode_Click(object sender, RoutedEventArgs e)
    {
        var node = new PrintNode();
        AddNodeToCanvas(node, lastMousePosition);
    }

    /// <summary>
    /// 将节点及其连接器添加到画布中，并设置初始位置。
    /// </summary>
    private void AddNodeToCanvas(Node node, Point position)
    {
        nodes.Add(node);
        nodeCanvas.Children.Add(node);
        Canvas.SetLeft(node, position.X);
        Canvas.SetTop(node, position.Y);

        // 为每个输入连接器创建视觉元素并更新其位置
        foreach (var input in node.Inputs)
        {
            var connector = CreateConnectorVisual(input);
            connectorVisuals[input] = connector;
            nodeCanvas.Children.Add(connector);
            UpdateConnectorPosition(connector, node, true, input.Index);
        }

        // 为每个输出连接器创建视觉元素并更新其位置
        foreach (var output in node.Outputs)
        {
            var connector = CreateConnectorVisual(output);
            connectorVisuals[output] = connector;
            nodeCanvas.Children.Add(connector);
            UpdateConnectorPosition(connector, node, false, output.Index);
        }
    }

    /// <summary>
    /// 更新连接器在画布上的位置，根据其类型（输入或输出）和索引调整坐标。
    /// </summary>
    private void UpdateConnectorPosition(Ellipse connector, Node node, bool isInput, int index)
    {
        var nodePos = node.Position;
        var x = isInput ? nodePos.X : nodePos.X + node.Width;
        var y = nodePos.Y + 30 + (index * 30); // 根据索引调整垂直位置
        Canvas.SetLeft(connector, x - connector.Width / 2);
        Canvas.SetTop(connector, y - connector.Height / 2);
    }

    /// <summary>
    /// 创建连接器的视觉表示（椭圆），并为其绑定鼠标事件。
    /// </summary>
    private Ellipse CreateConnectorVisual(Connector connector)
    {
        var ellipse = new Ellipse
        {
            Width = 10,
            Height = 10,
            Fill = Brushes.White,
            Stroke = Brushes.Gray,
            StrokeThickness = 1,
            Tag = connector,
            Cursor = Cursors.Hand
        };

        // 鼠标按下时开始拖拽连接线
        ellipse.MouseLeftButtonDown += (s, e) =>
        {
            if (connector.Type == ConnectorType.Output)
            {
                startConnector = connector;
                isDraggingConnection = true;
                connectionLine.Visibility = Visibility.Visible;
                var pos = e.GetPosition(nodeCanvas);
                connectionLine.X1 = pos.X;
                connectionLine.Y1 = pos.Y;
            }
        };

        // 鼠标松开时完成连接操作
        ellipse.MouseLeftButtonUp += (s, e) =>
        {
            if (isDraggingConnection && connector.Type == ConnectorType.Input)
            {
                // 检查输入端是否已经有连接
                if (connector.ConnectedTo != null)
                {
                    MessageBox.Show("该输入端已经连接，请先断开现有连接。", "连接错误", MessageBoxButton.OK, MessageBoxImage.Warning);
                    isDraggingConnection = false;
                    connectionLine.Visibility = Visibility.Collapsed;
                    return;
                }

                // 创建连接关系并更新连接线
                startConnector.ConnectedTo = connector;
                connector.ConnectedTo = startConnector;

                var line = new Line
                {
                    Stroke = Brushes.White,
                    StrokeThickness = 2,
                    Tag = new Tuple<Connector, Connector>(startConnector, connector)
                };

                startConnector.ParentNode.ConnectionLines.Add(line);
                connector.ParentNode.ConnectionLines.Add(line);
                nodeCanvas.Children.Add(line);

                startConnector.ParentNode.UpdateConnectionLines();
            }
            isDraggingConnection = false;
            connectionLine.Visibility = Visibility.Collapsed;
        };

        return ellipse;
    }

    /// <summary>
    /// 处理鼠标移动事件，实时更新拖拽中的连接线位置。
    /// </summary>
    private void nodeCanvas_MouseMove(object sender, MouseEventArgs e)
    {
        if (isDraggingConnection)
        {
            var pos = e.GetPosition(nodeCanvas);
            connectionLine.X2 = pos.X;
            connectionLine.Y2 = pos.Y;
        }
    }

    /// <summary>
    /// 点击运行按钮后，遍历所有节点并执行其逻辑。
    /// </summary>
    private void btnRun_Click(object sender, RoutedEventArgs e)
    {
        foreach (var node in nodes)
        {
            node.Execute();
        }
    }
}