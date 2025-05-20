using System.Windows;
using System.Windows.Controls;

namespace NodeGraph
{
    public class FloatNode : Node
    {
        private TextBox valueTextBox;
        public float Value { get; private set; }

        public FloatNode()
        {
            Title = "Float";
            titleBlock.Text = Title;

            valueTextBox = new TextBox
            {
                Width = 60,
                Margin = new Thickness(0, 5, 0, 0)
            };
            valueTextBox.TextChanged += (s, e) =>
            {
                if (float.TryParse(valueTextBox.Text, out float result))
                {
                    Value = result;
                }
            };

            contentPanel.Children.Add(valueTextBox);

            // 添加输出连接器
            var output = new Connector(this, ConnectorType.Output, 0);
            Outputs.Add(output);

            this.Width = 100;
            this.Height = 80;
        }

        public override void Execute()
        {
            // Float节点不需要执行任何操作
        }
    }
} 