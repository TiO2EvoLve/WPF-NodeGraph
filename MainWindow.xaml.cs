using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NodeGraph;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private List<Node> nodes = new List<Node>();
    private Connector startConnector;
    private Line connectionLine;
    private bool isDraggingConnection = false;
    private Dictionary<Connector, Ellipse> connectorVisuals = new Dictionary<Connector, Ellipse>();
    private Point lastMousePosition;

    public MainWindow()
    {
        InitializeComponent();
        InitializeConnectionLine();
        nodeCanvas.MouseMove += nodeCanvas_MouseMove;
    }

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

    private void nodeCanvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        lastMousePosition = e.GetPosition(nodeCanvas);
        nodeContextMenu.IsOpen = true;
    }

    private void CreateFloatNode_Click(object sender, RoutedEventArgs e)
    {
        var node = new FloatNode();
        AddNodeToCanvas(node, lastMousePosition);
    }

    private void CreateAddNode_Click(object sender, RoutedEventArgs e)
    {
        var node = new AddNode();
        AddNodeToCanvas(node, lastMousePosition);
    }

    private void CreatePrintNode_Click(object sender, RoutedEventArgs e)
    {
        var node = new PrintNode();
        AddNodeToCanvas(node, lastMousePosition);
    }

    private void AddNodeToCanvas(Node node, Point position)
    {
        nodes.Add(node);
        nodeCanvas.Children.Add(node);
        Canvas.SetLeft(node, position.X);
        Canvas.SetTop(node, position.Y);

        // 为每个连接器添加鼠标事件
        foreach (var input in node.Inputs)
        {
            var connector = CreateConnectorVisual(input);
            connectorVisuals[input] = connector;
            nodeCanvas.Children.Add(connector);
            UpdateConnectorPosition(connector, node, true, input.Index);
        }

        foreach (var output in node.Outputs)
        {
            var connector = CreateConnectorVisual(output);
            connectorVisuals[output] = connector;
            nodeCanvas.Children.Add(connector);
            UpdateConnectorPosition(connector, node, false, output.Index);
        }
    }

    private void UpdateConnectorPosition(Ellipse connector, Node node, bool isInput, int index)
    {
        var nodePos = node.Position;
        var x = isInput ? nodePos.X : nodePos.X + node.Width;
        var y = nodePos.Y + 30 + (index * 30); // 根据索引调整垂直位置
        Canvas.SetLeft(connector, x - connector.Width / 2);
        Canvas.SetTop(connector, y - connector.Height / 2);
    }

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

                // 创建连接
                startConnector.ConnectedTo = connector;
                connector.ConnectedTo = startConnector;

                // 创建连接线
                var line = new Line
                {
                    Stroke = Brushes.White,
                    StrokeThickness = 2,
                    Tag = new Tuple<Connector, Connector>(startConnector, connector)
                };

                // 将连接线添加到两个节点
                startConnector.ParentNode.ConnectionLines.Add(line);
                connector.ParentNode.ConnectionLines.Add(line);
                nodeCanvas.Children.Add(line);

                // 更新连接线位置
                startConnector.ParentNode.UpdateConnectionLines();
            }
            isDraggingConnection = false;
            connectionLine.Visibility = Visibility.Collapsed;
        };

        return ellipse;
    }

    private void nodeCanvas_MouseMove(object sender, MouseEventArgs e)
    {
        if (isDraggingConnection)
        {
            var pos = e.GetPosition(nodeCanvas);
            connectionLine.X2 = pos.X;
            connectionLine.Y2 = pos.Y;
        }
    }

    private void btnRun_Click(object sender, RoutedEventArgs e)
    {
        foreach (var node in nodes)
        {
            node.Execute();
        }
    }
}