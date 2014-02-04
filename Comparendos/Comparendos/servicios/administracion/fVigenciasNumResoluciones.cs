﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using LibreriasSintrat.ServiciosDocumentos;
using LibreriasSintrat.utilidades;

using System.Collections;

namespace Comparendos.servicios.administracion
{
    enum TipoOperacion
    {
        editar = 1,        
        nuevo = 2
    }

    public partial class fVigenciasNumResoluciones : Form
    {
        ServiciosDocumentosService serviciosDocumentos;
        Funciones funciones;
        TipoOperacion operacionActual;

        public fVigenciasNumResoluciones()
        {
            InitializeComponent();
            serviciosDocumentos = WS.ServiciosDocumentosService();
            funciones = new Funciones();
            
            actualizarGrillaVigencias();
        }


        private void actualizarGrillaVigencias()
        {
            viewnumerosresolucion viewNumerosResolucion = new viewnumerosresolucion();
                       
            //object[] listaVigencias = serviciosDocumentos.buscarViewnumerosresolucion(viewNumerosResolucion);
            object[] listaVigencias = serviciosDocumentos.listarViewnumerosresolucion();

            if (listaVigencias != null)
            {
                DataTable dtVigencias = new DataTable();

                ArrayList campos = new ArrayList();
                campos.Add("ID_VIGENCIA = ID_VIGENCIA");
                campos.Add("ANO = Vigencia");
                campos.Add("NUM_RESOLUCION_INICIO = Núm. Resolución Inicio");
                campos.Add("NUM_RESOLUCION_ACTUAL = Núm. Resolución Actual");                

                dtVigencias = funciones.ToDataTableEnOrden(funciones.ObjectToArrayList(listaVigencias), campos);
                grdVigencias.DataSource = dtVigencias;

                if (dtVigencias != null && dtVigencias.Rows.Count > 0)
                {
                    funciones.configurarGrillaEnOrden(grdVigencias, campos);
                    grdVigencias.Columns[0].Visible = false;    //ID_VIGENCIA
                    grdVigencias.Select();
                }
            }
            else
            {
                grdVigencias.DataSource = null;
                MessageBox.Show("No se encontraron vigencias!");
            }
        }

        #region Eventos Formulario
            private void btnEditar_Click(object sender, EventArgs e)
            {
                if (grdVigencias.SelectedCells.Count > 0)
                {
                    operacionActual = TipoOperacion.editar;

                    //Actualizar información de registro actual
                    //grdVigencias_SelectionChanged(null, null);

                    //Habilitar botones
                    btnGuardar.Enabled = btnCancelar.Enabled = true;

                    //Habilitar controles
                    txtAno.Enabled = txtNumResolucionInicio.Enabled = true;
                }
                else
                    MessageBox.Show("Debe seleccionar un registro de vigencia!");
            }

            private void btnEliminar_Click(object sender, EventArgs e)
            {
                if (grdVigencias.SelectedCells.Count > 0)
                {
                    DialogResult confirmacionEliminar = MessageBox.Show(this, "¿Está seguro que desea eliminar el registro?", "Confirmación", MessageBoxButtons.YesNo);

                    if (confirmacionEliminar == System.Windows.Forms.DialogResult.Yes)
                    {
                        int filaActual = grdVigencias.SelectedCells[0].RowIndex;

                        vigencias objVigencia = new vigencias();                        

                        int idVigencia = int.Parse(grdVigencias.Rows[filaActual].Cells[0].Value.ToString());

                        //OBTENER EL REGISTRO DE VIGENCIA ACTUAL
                        objVigencia.ID_VIGENCIA = idVigencia;                        
                                                
                        if (serviciosDocumentos.eliminarVigencias(objVigencia))
                        {
                            MessageBox.Show("Registro eliminado correctamente!");
                            actualizarGrillaVigencias();
                        }
                        else
                            MessageBox.Show("Error al eliminar la vigencia!");                        
                    }
                }
                else
                    MessageBox.Show("Debe seleccionar un registro de vigencia!");
            }

            private void btnNuevaVigencia_Click(object sender, EventArgs e)
            {
                operacionActual = TipoOperacion.nuevo;
                
                //Habilitar botones
                btnGuardar.Enabled = btnCancelar.Enabled = true;

                //Habilitar controles
                txtAno.Enabled = txtNumResolucionInicio.Enabled = true;

                txtAno.Text = txtNumResolucionInicio.Text = "";
            }

            private void btnGuardar_Click(object sender, EventArgs e)
            {
                if (formularioValido())
                {
                    switch (operacionActual)
                    {
                        case TipoOperacion.editar:
                                if (grdVigencias.SelectedCells.Count > 0)
                                {
                                    int filaActual = grdVigencias.SelectedCells[0].RowIndex;

                                    vigencias objVigencia = new vigencias();

                                    int idVigencia = int.Parse(grdVigencias.Rows[filaActual].Cells[0].Value.ToString());                                    

                                    //OBTENER EL REGISTRO DE VIGENCIA ACTUAL
                                    objVigencia.ID_VIGENCIA = idVigencia;
                                    objVigencia = serviciosDocumentos.buscarPrimeroVigencias(objVigencia);

                                    if (objVigencia != null && objVigencia.ID_VIGENCIA > 0)
                                    {
                                        objVigencia.ANO = int.Parse(txtAno.Text);
                                        objVigencia.NUM_RESOLUCION_INICIO = int.Parse(txtNumResolucionInicio.Text);

                                        if (serviciosDocumentos.editarVigencias(objVigencia))
                                        {
                                            MessageBox.Show("Registro actualizado!");
                                            actualizarGrillaVigencias();
                                        }
                                        else
                                            MessageBox.Show("Error al actualizar vicencia!");
                                    }
                                    else
                                        MessageBox.Show("Registro de vicencia no encontrado!");
                                }
                                else
                                    MessageBox.Show("Debe seleccionar un registro de vigencia!");
                            break;

                        case TipoOperacion.nuevo:
                                vigencias objVigenciaNew = new vigencias();
                                objVigenciaNew.ANO = int.Parse(txtAno.Text);
                                objVigenciaNew.NUM_RESOLUCION_INICIO = int.Parse(txtNumResolucionInicio.Text);
                                
                                objVigenciaNew = serviciosDocumentos.crearVigencias(objVigenciaNew);

                                if (objVigenciaNew!=null && objVigenciaNew.ID_VIGENCIA>0)
                                {
                                    MessageBox.Show("Registro ingresado correctamente!");
                                    actualizarGrillaVigencias();
                                }
                                else
                                    MessageBox.Show("Error al ingresar vigencia. Recuerde: no pueden existir dos vigencias con el mismo año.", "Año duplicado");
                            break;
                    }
                }
            }

            private void btnCancelar_Click(object sender, EventArgs e)
            {
                deshabilitarEdicion();
                grdVigencias.Select();
            }
        #endregion

            private void grdVigencias_SelectionChanged(object sender, EventArgs e)
            {
                if (grdVigencias.SelectedCells.Count > 0)
                {
                    deshabilitarEdicion();

                    int filaActual = grdVigencias.SelectedCells[0].RowIndex;

                    txtAno.Text = grdVigencias.Rows[filaActual].Cells[1].Value.ToString();
                    txtNumResolucionInicio.Text = grdVigencias.Rows[filaActual].Cells[2].Value.ToString();                    
                }
            }

            private void deshabilitarEdicion()
            {
                //Deshabilitar botones
                btnGuardar.Enabled = btnCancelar.Enabled = false;
                //Deshabilitar controles
                txtAno.Enabled = txtNumResolucionInicio.Enabled = false;
            }

            private bool formularioValido()
            {
                if (String.IsNullOrEmpty(txtAno.Text))
                {
                    MessageBox.Show("El año es obligatorio.");
                    txtAno.Focus();
                    return false;
                }

                if (String.IsNullOrEmpty(txtNumResolucionInicio.Text))
                {
                    MessageBox.Show("El número de inicio de resoluciones es obligatorio.");
                    txtNumResolucionInicio.Focus();
                    return false;
                }

                if (!funciones.esAnio(txtAno.Text))
                {
                    MessageBox.Show("El año debe ser un número positivo de cuatro dígitos.");
                    txtAno.Focus();
                    return false;
                }

                if (!(funciones.esNumero(txtNumResolucionInicio.Text) && int.Parse(txtNumResolucionInicio.Text)>=0))
                {
                    MessageBox.Show("Núm. Resolución Inicio debe ser un número mayor o igual a cero.");
                    txtNumResolucionInicio.Focus();
                    return false;
                }

                return true;
            }

            private void txtNumResolucionInicio_KeyPress(object sender, KeyPressEventArgs e)
            {
                funciones.esNumero(e);
            }

    }
}
