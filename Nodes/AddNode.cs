using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace NodeGraph
{
    public class AddNode : Node
    {
        public float Result { get; private set; }

        public AddNode()
        {
            Title = "Add";
            titleBlock.Text = Title;

            // 创建输入标签
            var inputPanel = new StackPanel { Margin = new Thickness(0, 5, 0, 0) };
            var input1Label = new TextBlock { Text = "A", Foreground = Brushes.White };
            var input2Label = new TextBlock { Text = "B", Foreground = Brushes.White, Margin = new Thickness(0, 5, 0, 0) };
            inputPanel.Children.Add(input1Label);
            inputPanel.Children.Add(input2Label);
            contentPanel.Children.Add(inputPanel);

            // 添加两个输入连接器
            var input1 = new Connector(this, ConnectorType.Input, 0);
            var input2 = new Connector(this, ConnectorType.Input, 1);
            Inputs.Add(input1);
            Inputs.Add(input2);

            // 添加输出连接器
            var output = new Connector(this, ConnectorType.Output, 0);
            Outputs.Add(output);

            this.Width = 100;
            this.Height = 120;
        }

        public override void Execute()
        {
            if (Inputs[0].ConnectedTo?.ParentNode is FloatNode input1 &&
                Inputs[1].ConnectedTo?.ParentNode is FloatNode input2)
            {
                Result = input1.Value + input2.Value;
            }
        }
    }
} 