﻿using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FacturaEmcoclavos
{
    public partial class Form1 : Form
    {
        private DataTable dt;
        public Form1()
        {
            InitializeComponent();
        }

        private void BtnAbrirExcel_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog() { Filter = "Excel Workbook|*.xlsx", Multiselect = false })
            {
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        Cursor.Current = Cursors.WaitCursor;
                        dt = new DataTable();
                        using (XLWorkbook workbook = new XLWorkbook(ofd.FileName))
                        {
                            bool isFisrtRow = true;
                            var rows = workbook.Worksheet(1).RowsUsed();
                            foreach (var row in rows)
                            {
                                if (isFisrtRow)
                                {
                                    //adding collumn
                                    foreach (IXLCell cell in row.Cells())
                                        dt.Columns.Add(cell.Value.ToString());
                                    isFisrtRow = false;
                                }
                                else
                                {
                                    dt.Rows.Add();
                                    int i = 0;
                                    foreach (IXLCell cell in row.Cells())
                                    {
                                        dt.Rows[dt.Rows.Count - 1][i++] = cell.Value.ToString();
                                    }
                                }
                            }
                            dataGridView1.DataSource = dt.DefaultView;
                            label1.Text = $"Total registros: {dataGridView1.RowCount}";
                            Cursor.Current = Cursors.Default;
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                }
            }
        }

        private void BtnBuscar_click(object sender, EventArgs e)
        {
            try
            {
                DataView dv = dataGridView1.DataSource as DataView;
                if (dv != null)
                    dv.RowFilter = textBox1.Text;
                label1.Text = $"Total registros: {dataGridView1.RowCount}";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)13)
                btnBuscar.PerformClick();
        }

        private void btnExportarExcel_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog sfd = new SaveFileDialog() { Filter = "Excel workbook|*.xlsx" })
            {
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        using (XLWorkbook workbook = new XLWorkbook())
                        {
                            workbook.Worksheets.Add(dt, "Archivo Excel");
                            workbook.SaveAs(sfd.FileName);
                        }
                        MessageBox.Show("Haz exportado el archvo excel con exito", "Message", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
    }
}
