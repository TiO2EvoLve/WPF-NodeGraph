using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace NodeGraph
{
    public class PrintNode : Node
    {
        private TextBlock resultText;

        public PrintNode()
        {
            Title = "Print";
            titleBlock.Text = Title;

            resultText = new TextBlock
            {
                Foreground = Brushes.White,
                Margin = new Thickness(0, 5, 0, 0)
            };

            contentPanel.Children.Add(resultText);

            // 添加输入连接器
            var input = new Connector(this, ConnectorType.Input, 0);
            Inputs.Add(input);

            this.Width = 100;
            this.Height = 80;
        }

        public override void Execute()
        {
            if (Inputs[0].ConnectedTo?.ParentNode is AddNode addNode)
            {
                resultText.Text = $"结果: {addNode.Result}";
            }
        }
    }
} 