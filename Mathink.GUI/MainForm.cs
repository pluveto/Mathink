using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WpfMath;
using WpfMath.Converters;

namespace Mathink.GUI
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }
        public string OutputPath { get; private set; } = "d.out.txt";
        private void txtIn_TextChanged(object sender, EventArgs e)
        {
            slRet.Text = string.Empty;
            var latex = txtIn.Text.Trim();
            if (latex == string.Empty) return;
            string d;
            try
            {
                (d, imgOut) = renderLatex(latex);
            }
            catch (Exception ex)
            {
                slErr.Text = "错误: " + ex.Message;
                return;
            }
            File.WriteAllText(OutputPath, d);
            using (var ms = new MemoryStream(imgOut))
            {
                this.picOut.Image = Image.FromStream(ms);
            }
            slErr.Text = string.Empty;
            slRet.Text = "渲染成功";

        }
        byte[] imgOut;



        private static (string, byte[]) renderLatex(string latex, bool onlyReturnDpath = true)
        {

            var parser = new TexFormulaParser();
            var formula = parser.Parse(latex);
            var renderer = formula.GetRenderer(TexStyle.Display, 20, "Arial");
            var pngBytes = formula.RenderToPng(20.0, 0.0, 0.0, "Arial");
            var geometry = renderer.RenderToGeometry(0, 0);
            var converter = new SVGConverter();
            var svgPathText = converter.ConvertGeometry(geometry);
            if (!onlyReturnDpath) return (AddSVGHeader(svgPathText), pngBytes);
            var tmp1 = svgPathText.Split(new string[] { "d=\"" }, StringSplitOptions.None)[1];
            var tmp2 = tmp1.Split(new string[] { "\"" }, StringSplitOptions.None)[0];
            return (tmp2, pngBytes);
        }
        private static string AddSVGHeader(string svgText)
        {
            var builder = new StringBuilder();
            builder.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\" ?>")
                .AppendLine("<svg xmlns=\"http://www.w3.org/2000/svg\" version=\"1.1\" >")
                .AppendLine(svgText)
                .AppendLine("</svg>");

            return builder.ToString();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            txtIn_TextChanged(null, null);
        }

        private void 文件ToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void 导入ToolStripMenuItem_Click(object sender, EventArgs e)
        {

            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Title = "打开文件";
            dialog.FileName = "";
            dialog.Filter = "平文本(.txt)|.txt|Markdown(.md)|.md|LaTex(.tex)|.tex|所有(*.*)|*.*";

            if (dialog.ShowDialog() == DialogResult.Cancel) return;
            var fileName = dialog.FileName;
            if (!File.Exists(fileName))
            {
                MessageBox.Show("文件不存在!", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            txtIn.Text = File.ReadAllText(fileName);

        }

        private void 保存图片ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var dialog = new SaveFileDialog();
            dialog.FileName = "";
            dialog.Filter = "PNG 格式|.png";
            dialog.Title = "保存";
            if (dialog.ShowDialog() == DialogResult.Cancel) return;

            var fileName = dialog.FileName;
            if (!fileName.EndsWith(".png")) fileName += ".png";
            System.IO.File.WriteAllBytes(fileName, imgOut);
            var ret = MessageBox.Show("保存成功, 打开所在目录吗?", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            if (ret == DialogResult.Yes)
            {
                string argument = "/select, \"" + fileName + "\"";
                System.Diagnostics.Process.Start("explorer.exe", argument);
            }
        }

        private void 保存SVGToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string svg;
            var latex = txtIn.Text.Trim();
            (svg, _) = renderLatex(latex, false);

            var dialog = new SaveFileDialog();
            dialog.FileName = "";
            dialog.Filter = "SVG 格式(.svg)|.svg";
            dialog.Title = "保存";
            if (dialog.ShowDialog() == DialogResult.Cancel) return;

            var fileName = dialog.FileName;
            if (!fileName.EndsWith(".svg")) fileName += ".svg";
            System.IO.File.WriteAllText(fileName, svg);
            var ret = MessageBox.Show("保存成功, 打开所在目录吗?", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            if (ret == DialogResult.Yes)
            {
                string argument = "/select, \"" + fileName + "\"";
                System.Diagnostics.Process.Start("explorer.exe", argument);
            }

        }

        private void 保存ToolStripMenuItem1_Click(object sender, EventArgs e)
        {

            var latex = txtIn.Text.Trim();

            var dialog = new SaveFileDialog();
            dialog.FileName = "";
            dialog.Filter = "平文本(.txt)|.txt";
            dialog.Title = "保存";
            if (dialog.ShowDialog() == DialogResult.Cancel) return;

            var fileName = dialog.FileName;
            if (!fileName.EndsWith(".txt")) fileName += ".txt";
            System.IO.File.WriteAllText(fileName, latex);
            var ret = MessageBox.Show("保存成功, 打开所在目录吗?", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            if (ret == DialogResult.Yes)
            {
                string argument = "/select, \"" + fileName + "\"";
                System.Diagnostics.Process.Start("explorer.exe", argument);
            }
        }

        private void 复制输出路径ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(Application.StartupPath + Path.DirectorySeparatorChar + OutputPath);
            slRet.Text = "复制成功!";
        }

        private void 打开程序目录ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("explorer.exe", Application.StartupPath);
        }
    }
}