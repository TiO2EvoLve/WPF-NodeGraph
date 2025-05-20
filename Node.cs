using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace NodeGraph
{
    public class Node : UserControl
    {
        public Point Position
        {
            get { return new Point(Canvas.GetLeft(this), Canvas.GetTop(this)); }
            set
            {
                Canvas.SetLeft(this, value.X);
                Canvas.SetTop(this, value.Y);
            }
        }

        public List<Connector> Inputs { get; protected set; } = new List<Connector>();
        public List<Connector> Outputs { get; protected set; } = new List<Connector>();
        public string Title { get; set; }
        public List<Line> ConnectionLines { get; } = new List<Line>();

        protected Border nodeBorder;
        protected TextBlock titleBlock;
        protected StackPanel contentPanel;

        private Point dragStartPoint;
        private bool isDragging = false;

        public Node()
        {
            // 创建主布局
            contentPanel = new StackPanel();
            
            // 创建标题
            titleBlock = new TextBlock
            {
                Foreground = Brushes.White,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 10)
            };
            contentPanel.Children.Add(titleBlock);

            // 创建边框
            nodeBorder = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(45, 45, 45)),
                BorderBrush = Brushes.Gray,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(5),
                Padding = new Thickness(10),
                Child = contentPanel
            };

            this.Content = nodeBorder;

            // 使节点可拖动
            this.MouseLeftButtonDown += Node_MouseLeftButtonDown;
            this.MouseMove += Node_MouseMove;
            this.MouseLeftButtonUp += Node_MouseLeftButtonUp;
        }

        private void Node_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isDragging = true;
            dragStartPoint = e.GetPosition(this);
            this.CaptureMouse();
        }

        private void Node_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                var currentPos = e.GetPosition(this);
                var deltaX = currentPos.X - dragStartPoint.X;
                var deltaY = currentPos.Y - dragStartPoint.Y;

                var newLeft = Canvas.GetLeft(this) + deltaX;
                var newTop = Canvas.GetTop(this) + deltaY;

                Canvas.SetLeft(this, newLeft);
                Canvas.SetTop(this, newTop);

                // 更新连接器位置
                var parent = this.Parent as Canvas;
                if (parent != null)
                {
                    foreach (var child in parent.Children)
                    {
                        if (child is Ellipse connector)
                        {
                            var tag = connector.Tag as Connector;
                            if (tag != null && (tag.ParentNode == this))
                            {
                                var isInput = tag.Type == ConnectorType.Input;
                                var x = isInput ? newLeft : newLeft + this.Width;
                                var y = newTop + 30 + (tag.Index * 30);
                                Canvas.SetLeft(connector, x - connector.Width / 2);
                                Canvas.SetTop(connector, y - connector.Height / 2);
                            }
                        }
                    }
                }

                // 更新连接线
                UpdateConnectionLines();
            }
        }

        private void Node_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isDragging = false;
            this.ReleaseMouseCapture();
        }

        public void UpdateConnectionLines()
        {
            foreach (var line in ConnectionLines)
            {
                var startConnector = line.Tag as Tuple<Connector, Connector>;
                if (startConnector != null)
                {
                    var start = startConnector.Item1;
                    var end = startConnector.Item2;

                    var startNode = start.ParentNode;
                    var endNode = end.ParentNode;

                    var startX = Canvas.GetLeft(startNode) + (start.Type == ConnectorType.Input ? 0 : startNode.Width);
                    var startY = Canvas.GetTop(startNode) + 30 + (start.Index * 30);
                    var endX = Canvas.GetLeft(endNode) + (end.Type == ConnectorType.Input ? 0 : endNode.Width);
                    var endY = Canvas.GetTop(endNode) + 30 + (end.Index * 30);

                    line.X1 = startX;
                    line.Y1 = startY;
                    line.X2 = endX;
                    line.Y2 = endY;
                }
            }
        }

        public virtual void Execute() { }
    }

    public class Connector
    {
        public Point Position { get; set; }
        public Node ParentNode { get; set; }
        public ConnectorType Type { get; set; }
        public Connector ConnectedTo { get; set; }
        public int Index { get; set; }

        public Connector(Node parent, ConnectorType type, int index)
        {
            ParentNode = parent;
            Type = type;
            Index = index;
        }
    }

    public enum ConnectorType
    {
        Input,
        Output
    }
} 