using ClosedXML.Excel;
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
        private DataTable dt, dt2, dt3;
        public Form1()
        {
            InitializeComponent();
            btnCalcularTotalesOperaciones.Visible = false;
            btnExportarExcel.Visible = false;
            btnCalcularFactura.Visible = false;
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
                                        dt.Rows[dt.Rows.Count - 1][i++] = cell.Value;
                                    }
                                }
                            }
                            dataGridView1.DataSource = dt.DefaultView;
                            label1.Text = $"Total registros: {dataGridView1.RowCount}";
                            btnCalcularTotalesOperaciones.Visible = true;
                            btnAbrirExcel.Visible = false;
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

        private void btnCalcularFactura_Click(object sender, EventArgs e)
        {
            try
            {
                dt3 = new DataTable();
                string[] columnas = new string[] { "Llave", "Consumo" };
                foreach (string c in columnas)
                {
                    dt3.Columns.Add(c);
                }
                for (int i=0;i<dt2.Rows.Count;i++)
                {
                    dt3.Rows.Add();
                    dt3.Rows[i][0] = dt2.Rows[i][0];
                    if (int.Parse(dt2.Rows[i][3].ToString()) > 21500)
                        dt3.Rows[dt3.Rows.Count - 1][1] = 21500;
                    else
                        dt3.Rows[dt3.Rows.Count - 1][1] = dt2.Rows[i][3];
                }

                dataGridView1.DataSource = dt3.DefaultView;
                label1.Text = $"Total registros: {dataGridView1.RowCount}";
                btnExportarExcel.Visible = true;
                btnCalcularFactura.Visible = false;
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCalcularTotalesOperaciones_Click(object sender, EventArgs e)
        {
            try
            {

                dt2 = new DataTable();
                string[] columnas = new string[] { "Llave", "Saldo inicial", "Saldo final", "Ventas", "Recargas", "Balance" };
                string llave = "0";
                int numRegistros = dt.Rows.Count;

                foreach (string c in columnas)
                {
                    dt2.Columns.Add(c);
                }
                for (int i = 0; i < numRegistros; i++)
                {
                    if (llave != dt.Rows[i][1].ToString())
                    {
                        dt2.Rows.Add();
                        llave = dt.Rows[i][1].ToString();
                        dt2.Rows[dt2.Rows.Count - 1][0] = llave;
                        dt2.Rows[dt2.Rows.Count - 1][3] = "0";
                        dt2.Rows[dt2.Rows.Count - 1][4] = "0";

                        if (dt2.Rows.Count > 1)// Guardar saldo final
                        {
                            dt2.Rows[dt2.Rows.Count - 2][2] = dt.Rows[i - 1][3];
                        }
                    }
                    switch (dt.Rows[i][0].ToString())
                    {
                        case "ASIGNACION DE CREDITO (4)                    ":
                            dt2.Rows[dt2.Rows.Count - 1][1] = dt.Rows[i][3];
                            break;
                        case "VENTA (1)                                    ":
                            dt2.Rows[dt2.Rows.Count - 1][3] = (int.Parse(dt2.Rows[dt2.Rows.Count - 1][3].ToString()) + int.Parse(dt.Rows[i][2].ToString()));
                            break;
                        case "RECARGA (3)                                  ":
                            dt2.Rows[dt2.Rows.Count - 1][4] = (int.Parse(dt2.Rows[dt2.Rows.Count - 1][4].ToString()) + int.Parse(dt.Rows[i][2].ToString()));
                            break;
                        default:
                            break;
                    }
                    if (i == numRegistros - 1)// Guardar saldo final para la ultima llave
                    {
                        dt2.Rows[dt2.Rows.Count - 1][2] = dt.Rows[i][3];
                    }
                }
                for (int i = 0; i < dt2.Rows.Count; i++)//Hacer balance = saldo final - (saldo inicial + recargas - ventas) // De tener los datos de operacion de las maquinas completos deberia dar cero.
                {
                    dt2.Rows[i][5] = (int.Parse(dt2.Rows[i][2].ToString()) - int.Parse(dt2.Rows[i][1].ToString()) - int.Parse(dt2.Rows[i][4].ToString()) + int.Parse(dt2.Rows[i][3].ToString()));
                }

                dataGridView1.DataSource = dt2.DefaultView;
                label1.Text = $"Total registros: {dataGridView1.RowCount}";
                btnCalcularFactura.Visible = true;
                btnCalcularTotalesOperaciones.Visible = false;
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

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
                            workbook.Worksheets.Add(dt, "Consumo");
                            workbook.Worksheets.Add(dt2, "Total Operaciones");
                            workbook.Worksheets.Add(dt3, "Factura");
                            workbook.SaveAs(sfd.FileName);
                        }
                        MessageBox.Show("Haz exportado el archvo excel con exito", "Message", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        btnExportarExcel.Visible = false;
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
