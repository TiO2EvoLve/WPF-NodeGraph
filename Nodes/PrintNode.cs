using System;
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
            if (Inputs[0].ConnectedTo?.ParentNode is INodeOutput inputNode)
            {
                // 将结果显示在控制台上
                float result = inputNode.GetValue();
                Console.WriteLine($"结果: {result}");
                
                // 如果需要，仍然可以在界面上显示结果
                resultText.Text = $"结果: {result}";
            }
        }
    }
}