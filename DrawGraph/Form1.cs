﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using DrawGraph.Parser;

namespace DrawGraph
{
    public partial class Form1 : Form
    {
        Graphics g;
        Pen p;
        Func<double, double> func;

        double x_min = -30, x_gap = 0.1f;

        public Form1()
        {
            InitializeComponent();

            func = (x) => Math.Pow(x, 2);
            trackBar1.Value = (int)-x_min;
            g = panel1.CreateGraphics();
            p = Pens.Blue;
            
            panel1.Paint += Panel1_Paint;
        }

        private void Panel1_Paint(object sender, PaintEventArgs e)
        {
            g = e.Graphics;
            DrawGrid();
            DrawFunction();
        }

        void DrawGrid()
        {
            g.DrawLine(Pens.Black, panel1.Width / 2, 0, panel1.Width / 2, panel1.Height);
            g.DrawLine(Pens.Black, 0, panel1.Height / 2, panel1.Width, panel1.Height / 2);
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            x_min = -trackBar1.Value;
            panel1.Invalidate();
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                var parsed = Builder.Parse(textBox1.Text);
                func = x => Evaluate(parsed, x);
                panel1.Invalidate();

                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        void DrawFunction()
        {
            double ratio = panel1.Width / (-2 * x_min);
            double prev_x = x_min, prev_y = func(x_min);
            double y_max = panel1.Height / 2 * ratio;
            double y_min = -y_max;
            for (var x = x_min + x_gap; x < -x_min; x += x_gap)
            {
                var y = func(x);

                if ((y < y_max && prev_y < y_max) && (y > y_min && prev_y > y_min))
                    DrawLine(prev_x, prev_y, x, y);

                prev_x = x;
                prev_y = y;
            }

            void DrawLine(double x1, double y1, double x2, double y2)
                => g.DrawLine(
                    p,
                    (float)(panel1.Width / 2 + x1 * ratio),
                    (float)(panel1.Height / 2 - y1 * ratio),
                    (float)(panel1.Width / 2 + x2 * ratio),
                    (float)(panel1.Height / 2 - y2 * ratio));
        }

        double Evaluate(Node node, double x)
        {
            if (node is BinaryNode bin)
            {
                double l = Evaluate(bin.Left, x), r = Evaluate(bin.Right, x);
                switch (bin.Operator.Code)
                {
                    case "+":
                        return l + r;
                    case "-":
                        return l - r;
                    case "*":
                        return l * r;
                    case "/":
                        return l / r;
                    case "%":
                        return l % r;
                    case "^":
                        return Math.Pow(l, r);
                }
            }
            else if (node is UnaryNode un)
            {
                var val = Evaluate(un.Expression, x);
                if (un.Operator.Code == "-")
                    return -val;
                else
                    return val;
            }
            else if (node is NumericNode num)
            {
                return double.Parse(num.Value.Code);
            }
            else if (node is IdentifierNode id)
            {
                if (id.Value.Code == "x")
                    return x;
                else
                    throw new Exception("모르는 변수가 있음");
            }
            throw new Exception("읭");
        }
    }
}
