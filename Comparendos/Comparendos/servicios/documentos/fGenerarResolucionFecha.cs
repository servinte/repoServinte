﻿using System; using TransitoPrincipal; using LibreriasSintrat.utilidades;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using LibreriasSintrat.ServiciosAccesoriasComp;
using LibreriasSintrat.ServiciosGeneralesComp;
using LibreriasSintrat.ServiciosDocumentos;
using LibreriasSintrat.ServiciosUsuarios;
using LibreriasSintrat.ServiciosGenerales;
using LibreriasSintrat.ServiciosConfiguraciones;
using Comparendos.utilidades; 
using LibreriasSintrat;

using Word = Microsoft.Office.Interop.Word;
using System.Web.UI.WebControls;

namespace Comparendos.servicios.documentos
{
    public partial class fGenerarResolucionFecha : Form
    {
        private usuarios myUsuario;
        
        private infractorComp myInfractor;
        
        private Object[] myComparendos;
        
        private int numResolucion;
        
        private plantilla myPlantilla;
        private plantilla myPlantillaAlcoholemia;
        private plantilla myPlantillaMototaxismo;

        private tipoResolucionComp myTipoResolucion;
        
        Funciones funciones;
        
        ServiciosGeneralesService serviciosGenerales;
        ServiciosDocumentosService serviciosDocumentos;
        ServiciosGeneralesCompService serviciosGeneralesComp;
        ServiciosConfiguracionesService serviciosConfiguraciones;

        Log log = new Log();
        

        public fGenerarResolucionFecha(usuarios user)
        {
            InitializeComponent();
                                    
            serviciosGenerales = WS.ServiciosGeneralesService();            
            serviciosDocumentos = WS.ServiciosDocumentosService();
            serviciosGeneralesComp = WS.ServiciosGeneralesCompService();
            serviciosConfiguraciones = WS.ServiciosConfiguracionesService();

            funciones = new Funciones();

            myUsuario = user;
            numResolucion = -1;
        }

        private void fGenerarResolucionFecha_Load(object sender, EventArgs e)
        {
            try
            {
                btnGenerarResolucion.Enabled = false;
                cargarCombos();
                dtFechaInicio.Focus();
            }
            catch (Exception exp)
            {
                log.escribirError(exp.ToString(), this.GetType());
                MessageBox.Show("Error desconocido realizando la funcionalidad! " + exp.Message);
                //Console.WriteLine(exp.Message);
                //Console.WriteLine(exp.StackTrace);
            }
        }

        public void cargarCombos()
        {
            //Tipo Acción
            ListItem itemNinguna = new ListItem("Ninguna", AccionAlcoholemia.Ninguna.ToString());
            ListItem itemSuspension = new ListItem("Suspensión", AccionAlcoholemia.Suspension.ToString());
            ListItem itemCancelacion = new ListItem("Cancelación", AccionAlcoholemia.Cancelacion.ToString());
            cmbTipoAccionAlcoholemia.Items.Add(itemNinguna);
            cmbTipoAccionAlcoholemia.Items.Add(itemSuspension);
            cmbTipoAccionAlcoholemia.Items.Add(itemCancelacion);
            cmbTipoAccionAlcoholemia_SelectedIndexChanged(null, null);


            plantilla myTmpPlantilla = new plantilla();
            Object[] arreglo;

            //Resolución de Imposición
            myTmpPlantilla.IDTIPORESOLUCION = 5;
            arreglo = (Object[])serviciosDocumentos.buscarPlantillasComp(myTmpPlantilla);

            funciones.llenarCombo(cmbPlantillaAlcoholemia, arreglo);
            funciones.llenarCombo(cmbPlantillaMotoTaxi, arreglo);
            funciones.llenarCombo(cmbPlantillaImposicion, arreglo);            
            
        }        

        public void cargarComparendos()
        {
            limpiarComparendos();

            String filtroEstados;
            String[] estados;
            Object[] tmpComparendos;

            viewComparendosResolSancionComp myCompResoSanc = new viewComparendosResolSancionComp();            

            filtroEstados = serviciosGeneralesComp.obtenerEstadosResolSancion();

            if (!string.IsNullOrEmpty(filtroEstados))
            {
                estados = filtroEstados.Split(',');

                for (int i = 0; i < estados.Length; i++)
                {
                    int idEstadoComparendo = int.Parse(estados[i]);
                                        
                    myCompResoSanc.ID_ESTADO = idEstadoComparendo;
                    tmpComparendos = (Object[])serviciosGeneralesComp.buscarComparendosEnRangoFecha(funciones.convFormatoFecha(dtFechaInicio.Text, "MM/dd/yyyy"), funciones.convFormatoFecha(dtFechaFin.Text, "MM/dd/yyyy"), myCompResoSanc);
                    myComparendos = funciones.unirArrayObject(myComparendos, tmpComparendos);
                }

                if (myComparendos != null && myComparendos.Length > 0)
                    mostrarComparendos();
                else
                {
                    MessageBox.Show("No se encontraron comparendos en las fechas indicadas.", "Información no encontrada");
                }
            }
            else
            {
                MessageBox.Show("Debe configurar los estados de resolución de sanción en la base de datos.", "Información no encontrada");
            }
        }

        public void mostrarComparendos()
        {
            DataTable dt = new DataTable();
            ArrayList campos = new ArrayList();

            campos.Add("NUMEROCOMPARENDO = NUMEROCOMPARENDO");
            campos.Add("FECHACOMPARENDO = FECHACOMPARENDO");
            campos.Add("NUEVO_CODIGO = NUEVO_CODIGO");
            campos.Add("ESTADO = ESTADO");
            campos.Add("PLACA = PLACA");
            campos.Add("DESCRIPCION = DESCRIPCION");
            campos.Add("COD_INFRACCION = COD_INFRACCION");

            campos.Add("ID = ID");
            campos.Add("ID_COMPARENDO = ID_COMPARENDO");
            campos.Add("ID_ESTADO = ID_ESTADO");
            campos.Add("ID_INFRACTOR = ID_INFRACTOR");
            campos.Add("ID_SERVICIO = ID_SERVICIO");
            campos.Add("IDINFRACCION = IDINFRACCION");

            dt = funciones.ToDataTableEnOrden(funciones.ObjectToArrayList(myComparendos), campos);
            dtGrViewComparendos.DataSource = dt;

            if (dt != null && dt.Rows.Count > 0)
            {
                funciones.configurarGrilla(dtGrViewComparendos, campos);

                //Ocultar campos
                dtGrViewComparendos.Columns["ID"].Visible = false;
                dtGrViewComparendos.Columns["ID_COMPARENDO"].Visible = false;
                dtGrViewComparendos.Columns["ID_ESTADO"].Visible = false;
                dtGrViewComparendos.Columns["ID_INFRACTOR"].Visible = false;
                dtGrViewComparendos.Columns["ID_SERVICIO"].Visible = false;
                dtGrViewComparendos.Columns["IDINFRACCION"].Visible = false;
            }
        }

        public void limpiarComparendos()
        {
            dtGrViewComparendos.DataSource = null;
            myComparendos = null;
        }





        private void btnGenerarResolucion_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;

            try
            {
                if (validarDatos())
                {
                    if (dtGrViewComparendos.SelectedCells.Count > 0)
                    {
                        numResolucion = int.Parse(txtNumeroResolucion.Text);

                        DataGridViewSelectedRowCollection comparendosSeleccionados = dtGrViewComparendos.SelectedRows;

                        int totalComparendos = comparendosSeleccionados.Count;
                        int countResolucionesGeneradas = 0;

                        bool abrirArchivo = true;
                        if (totalComparendos > 1)
                            abrirArchivo = false;

                        for (int i = 0; i < totalComparendos; i++)
                        {
                            viewComparendosResolSancionComp infoComparendo = (viewComparendosResolSancionComp)funciones.listToViewCompResolSanc(comparendosSeleccionados[i]);
                            resolucionesComparendoComp myResolucionComparendo = new resolucionesComparendoComp();

                            GenerarResoluciones generarResolucion = new GenerarResoluciones();

                            myInfractor = new infractorComp();
                            myInfractor.ID_INFRACTOR = infoComparendo.ID_INFRACTOR;
                            myInfractor = serviciosGeneralesComp.buscarInfractor(myInfractor);

                            string fechaResolucion = funciones.convFormatoFecha(dtPickFechaResolucion.Text, "MM/dd/yyyy");

                            myResolucionComparendo.IDUSUARIO = myUsuario.ID_USUARIO;
                            myResolucionComparendo.FECHAAUDIENCIA = funciones.convFormatoFecha(dtPickFechaAudiencia.Text, "MM/dd/yyyy");
                            myResolucionComparendo.FECHA = fechaResolucion;
                            myResolucionComparendo.MOTIVO = txtMotivo.Text;
                            myResolucionComparendo.CONSIDERACIONJURIDICA = txtConsidJurid.Text;
                            myResolucionComparendo.ID_TIPORESOLUCION = myPlantilla.IDTIPORESOLUCION;

                            //SUSPENSIÒN / CANCELACIÒN ALCOHOLEMIA - Generar Resolución adicional (si aplica)
                            if (infoComparendo.NUEVO_CODIGO.Equals("E3"))
                            {
                                if (rbtPorDefecto.Checked)
                                {
                                    comparendoComp myComparendo = new comparendoComp();
                                    myComparendo.ID_COMPARENDO = infoComparendo.ID_COMPARENDO;

                                    myComparendo = serviciosGeneralesComp.searchOneComparendo(myComparendo);

                                    if (myComparendo != null && myComparendo.ID_COMPARENDO >= 0)
                                    {
                                        estadoAlcoholemia estadoAlcohol = new estadoAlcoholemia();
                                        estadoAlcohol = serviciosGeneralesComp.getFechaSuspension(null, int.Parse(myComparendo.VALORALCOLEMIA.ToString()), true);
                                        if (estadoAlcohol != null && estadoAlcohol.ID > 0)
                                        {
                                            myResolucionComparendo.ACCION_ALCOHOLEMIA = (int)AccionAlcoholemia.Suspension;
                                            myResolucionComparendo.TIEMPOSUSPENSIONLIC = int.Parse(estadoAlcohol.SUSPENCIONINFERIOR);
                                        }
                                    }
                                }

                                if (rbtEspecificarAccion.Checked)
                                {
                                    myResolucionComparendo.ACCION_ALCOHOLEMIA = (int)cmbTipoAccionAlcoholemia.SelectedIndex;

                                    if (cmbTipoAccionAlcoholemia.SelectedIndex.Equals((int)AccionAlcoholemia.Suspension))
                                        myResolucionComparendo.TIEMPOSUSPENSIONLIC = int.Parse(txtTiempoSuspension.Text);
                                }

                                //Crear resolución alcoholemia
                                abrirArchivo = false;
                                generarResolucion.crearResolucion(abrirArchivo, "ResolucionInfractor.dotx", numResolucion, myInfractor, myTipoResolucion, myUsuario, fechaResolucion, myPlantillaAlcoholemia, infoComparendo, ref myResolucionComparendo);

                                //Actualizar número de resolución y contador de generadas
                                numResolucion++;
                                countResolucionesGeneradas++;
                            }

                            //SUSPENSIÒN / CANCELACIÒN MOTOTAXISMO - Generar Resolución adicional (si aplica)
                            if (infoComparendo.NUEVO_CODIGO.Equals("D12"))
                            {
                                //Buscar si el mismo infractor ha cometido esta infracción otras veces en un periodo no mayor a un año
                                viewComparendosResolSancionComp tmpViewComparendo = new viewComparendosResolSancionComp();
                                tmpViewComparendo.IDINFRACCION = infoComparendo.IDINFRACCION;
                                tmpViewComparendo.ID_INFRACTOR = infoComparendo.ID_INFRACTOR;

                                Object[] tmpComparendos = (Object[])serviciosGeneralesComp.buscarComparendosEnRangoFecha(funciones.convFormatoFecha(dtPickFechaResolucion.Value.AddYears(-1).ToString(), "MM/dd/yyyy"), fechaResolucion, tmpViewComparendo);

                                if (tmpComparendos != null)
                                {
                                    //Si es la primera vez, no se hace nada
                                    if (tmpComparendos.Length == 1)
                                    {
                                        myResolucionComparendo.ACCION_ALCOHOLEMIA = (int)AccionAlcoholemia.Ninguna;
                                        myResolucionComparendo.TIEMPOSUSPENSIONLIC = 0;
                                    }

                                    //Si es la segunda vez, se suspende la licencia por 6 meses
                                    if (tmpComparendos.Length == 2)
                                    {
                                        myResolucionComparendo.ACCION_ALCOHOLEMIA = (int)AccionAlcoholemia.Suspension;
                                        myResolucionComparendo.TIEMPOSUSPENSIONLIC = 6;
                                    }

                                    //Si es la tercera vez, se cancela la licencia
                                    if (tmpComparendos.Length >= 3)
                                    {
                                        myResolucionComparendo.ACCION_ALCOHOLEMIA = (int)AccionAlcoholemia.Cancelacion;
                                        myResolucionComparendo.TIEMPOSUSPENSIONLIC = 0;
                                    }
                                }

                                //Crear resolución mototaxismo
                                abrirArchivo = false;
                                generarResolucion.crearResolucion(abrirArchivo, "ResolucionInfractor.dotx", numResolucion, myInfractor, myTipoResolucion, myUsuario, fechaResolucion, myPlantillaMototaxismo, infoComparendo, ref myResolucionComparendo);

                                //Actualizar número de resolución y contador de generadas
                                numResolucion++;
                                countResolucionesGeneradas++;
                            }


                            //Crear resolución Imposición
                            //Limpiar los campos de Acción en caso de Alcoholemia
                            myResolucionComparendo.ACCION_ALCOHOLEMIA = (int)AccionAlcoholemia.Ninguna;
                            myResolucionComparendo.TIEMPOSUSPENSIONLIC = 0;
                            generarResolucion.crearResolucion(abrirArchivo, "ResolucionInfractor.dotx", numResolucion, myInfractor, myTipoResolucion, myUsuario, fechaResolucion, myPlantilla, infoComparendo, ref myResolucionComparendo);

                            //Actualizar número de resolución y contador de generadas
                            numResolucion++;
                            countResolucionesGeneradas++;
                        }


                        string message = "";
                        message = countResolucionesGeneradas + " resolucion(es) creada(s) de " + totalComparendos + " comparendo(s) seleccionado(s).";
                        if (!abrirArchivo)
                            message += " Los archivos han sido almacenados en: " + serviciosConfiguraciones.leerRegistroIni("RESOLUCIONES");

                        MessageBox.Show(message, "Terminado", 0, MessageBoxIcon.Information);
                        //cargarComparendosInfractor();
                        cargarComparendos();
                    }
                    else
                    {
                        MessageBox.Show("Debe seleccionar al menos un comparendo!");
                        dtGrViewComparendos.Focus();
                    }
                }
            }
            catch (Exception exp)
            {
                log.escribirError(exp.ToString(), this.GetType());
                MessageBox.Show("Error desconocido realizando la funcionalidad! " + exp.Message + " -Por favor cierre los documentos de resoluciones abiertos.");
            }

            this.Cursor = Cursors.Arrow;
        }
        







        //private void btnGenerarResolucion_Click(object sender, EventArgs e)
        //{
        //    this.Cursor = Cursors.WaitCursor;

        //    try
        //    {
        //        viewComparendosResolSancionComp infoComparendo = new viewComparendosResolSancionComp();
        //        resolucionesComparendoComp myResolucionComparendo = new resolucionesComparendoComp();
        //        resolucionInfraccionesComp myResolucionInfraccion = new resolucionInfraccionesComp();
        //        infracionComparendoComp myInfraccionComparendo = new infracionComparendoComp();//no uso
        //        historicoEstadosComp myHistoricoComparendos = new historicoEstadosComp();

        //        String nombreResolucion;

        //        if (validarDatos())
        //        {
        //            if (dtGrViewComparendos.SelectedCells.Count > 0)
        //            {
        //                numResolucion = int.Parse(txtNumeroResolucion.Text);
        //                nombreResolucion = "";

        //                DataGridViewSelectedRowCollection comparendosSeleccionados = dtGrViewComparendos.SelectedRows;

        //                int totalComparendos = comparendosSeleccionados.Count;
        //                int countResolucionesGeneradas = 0;

        //                for (int i = 0; i < totalComparendos; i++)
        //                {
        //                    infoComparendo = (viewComparendosResolSancionComp)funciones.listToViewCompResolSanc(comparendosSeleccionados[i]);

        //                    myResolucionComparendo = new resolucionesComparendoComp();
        //                    myResolucionInfraccion = new resolucionInfraccionesComp();
        //                    myInfractor = new infractorComp();

        //                    myInfractor.ID_INFRACTOR = infoComparendo.ID_INFRACTOR;
        //                    myInfractor = serviciosGeneralesComp.buscarInfractor(myInfractor);

        //                    myResolucionComparendo.IDUSUARIO = myUsuario.ID_USUARIO;
        //                    myResolucionComparendo.FECHAAUDIENCIA = funciones.convFormatoFecha(dtPickFechaAudiencia.Text, "MM/dd/yyyy");
        //                    myResolucionComparendo.FECHA = funciones.convFormatoFecha(dtPickFechaResolucion.Text, "MM/dd/yyyy");
        //                    myResolucionComparendo.MOTIVO = txtMotivo.Text;
        //                    myResolucionComparendo.CONSIDERACIONJURIDICA = txtConsidJurid.Text;
        //                    myResolucionComparendo.ID_TIPORESOLUCION = myPlantilla.IDTIPORESOLUCION;

        //                    numResolucion = serviciosDocumentos.obtenerNumeroResolucionComp(numResolucion);
        //                    myResolucionComparendo.NUMERO = numResolucion.ToString();

        //                    nombreResolucion = funciones.quitarEspacios(myInfractor.NROIDENTIFICACION.ToString());
        //                    nombreResolucion += "_" + myResolucionComparendo.NUMERO + "_" + myTipoResolucion.DES_TIPORESOLUCION;
        //                    myResolucionComparendo.NOMBRE = nombreResolucion;


        //                    //SE GENERA LA RESOLUCIÓN Y SE SUBE AL SERVIDOR
        //                    generarResolucion(myResolucionComparendo, infoComparendo);


        //                    //fija el contenido de la plantilla modificada y registra la resolucion
        //                    myResolucionComparendo.CONTENIDO = "<t>" + myPlantilla.ENCABEZADO + "<->\n\n\n";
        //                    myResolucionComparendo.CONTENIDO += myPlantilla.CONTENIDO;

        //                    myResolucionComparendo = serviciosDocumentos.insertarResolucionComparendo(myResolucionComparendo, WS.MyUsuario.ID_USUARIO, WS.Funciones().obtenerDirIp(), WS.Funciones().obtenerHostName());


        //                    //ACTUALIZAR INFRACCION COMPARENDO
        //                    myInfraccionComparendo = new infracionComparendoComp();
        //                    myInfraccionComparendo.IDCOMPARENDO = infoComparendo.ID_COMPARENDO;
        //                    myInfraccionComparendo.IDINFRACCION = infoComparendo.IDINFRACCION; //Se obtiene el id de infracComp
        //                    myInfraccionComparendo = serviciosGeneralesComp.getInfraccionComparendo(myInfraccionComparendo);

        //                    //ResolucionesInfracciones
        //                    myResolucionInfraccion.IDRESOLUCION = myResolucionComparendo.ID;
        //                    myResolucionInfraccion.IDINFRACCION = myInfraccionComparendo.ID;
        //                    myResolucionInfraccion = serviciosDocumentos.insertarResolucionInfracciones(myResolucionInfraccion, WS.MyUsuario.ID_USUARIO, WS.Funciones().obtenerDirIp(), WS.Funciones().obtenerHostName());

        //                    //Se cambia el estado del comparendo en la base de datos
        //                    myInfraccionComparendo.IDESTADO = myTipoResolucion.ID_NUEVOESTADO;
        //                    serviciosGeneralesComp.cambiarEstadoComp(myInfraccionComparendo, WS.MyUsuario.ID_USUARIO, WS.Funciones().obtenerDirIp(), WS.Funciones().obtenerHostName());

        //                    //se pone el nuevo estado en historico estados
        //                    myHistoricoComparendos = new historicoEstadosComp();
        //                    myHistoricoComparendos.ID_ESTADO = myTipoResolucion.ID_NUEVOESTADO;
        //                    myHistoricoComparendos.ID_INFRACCIONCOMPARENDO = myInfraccionComparendo.ID;
        //                    myHistoricoComparendos.IDUSUARIO = myUsuario.ID_USUARIO;
        //                    myHistoricoComparendos.FECHA = funciones.convFormatoFecha(dtPickFechaResolucion.Text, "MM/dd/yyyy");
        //                    serviciosGeneralesComp.insertarHistoricoEstadosComp(myHistoricoComparendos);

        //                    numResolucion++;
        //                    countResolucionesGeneradas++;
        //                }

        //                MessageBox.Show(countResolucionesGeneradas + " resolucion(es) creada(s) de " + totalComparendos + " comparendo(s) seleccionado(s). Los archivos han sido almacenados en: " + serviciosConfiguraciones.leerRegistroIni("RESOLUCIONES"), "Terminado", 0, MessageBoxIcon.Information);
        //                cargarComparendos();
        //            }
        //            else
        //            {
        //                MessageBox.Show("Debe seleccionar al menos un comparendo!");
        //                dtGrViewComparendos.Focus();
        //            }
        //        }
        //    }
        //    catch (Exception exp)
        //    {
        //        log.escribirError(exp.ToString(), this.GetType());
        //        MessageBox.Show("Error desconocido realizando la funcionalidad! " + exp.Message);
        //        //Console.WriteLine(exp.Message);
        //        //Console.WriteLine(exp.StackTrace);
        //    }

        //    this.Cursor = Cursors.Arrow;
        //}

        //private void generarResolucion(resolucionesComparendoComp myResolucionesComparendo, viewComparendosResolSancionComp infoComparendo)
        //{
        //    String param;
            
        //    Object[] parametros = new Object[5];
        //    param = "FECHARESOLUCION = " + DateTime.Parse(myResolucionesComparendo.FECHA, System.Globalization.CultureInfo.InvariantCulture).ToString("dd/MM/yyyy");
        //    parametros[0] = param;
        //    param = "NUMRESOLUCION = " + myResolucionesComparendo.NUMERO;
        //    parametros[1] = param;
        //    param = "FECHAAUDIENCIA = " + DateTime.Parse(myResolucionesComparendo.FECHAAUDIENCIA, System.Globalization.CultureInfo.InvariantCulture).ToString("dd/MM/yyyy");
        //    parametros[2] = param;
        //    param = "MOTIVO = " + myResolucionesComparendo.MOTIVO;
        //    parametros[3] = param;
        //    param = "CONSIDERACIONJURIDICO = " + myResolucionesComparendo.CONSIDERACIONJURIDICA;
        //    parametros[4] = param;

        //    myPlantilla = serviciosDocumentos.procesarPlantillaValores(myPlantilla, parametros);

        //    parametros = new Object[2];
        //    param = "ID_INFRACTOR = " + myInfractor.ID_INFRACTOR;
        //    parametros[0] = param;
        //    param = "ID_COMPARENDO = " + infoComparendo.ID_COMPARENDO;
        //    parametros[1] = param;

        //    myPlantilla = serviciosDocumentos.procesarPlantillaComp(myPlantilla, parametros);

        //    String nombreDocumento = myResolucionesComparendo.NOMBRE;
        //    generarWord(myPlantilla, nombreDocumento, "ResolucionInfractor.dotx");
        //}

        //private void generarWord(plantilla myPlantilla, string nombreDocumento, string nombrePlantilla)
        //{
        //    //*****///// PARTE PARA LA GENERACION DE DOCUMENTO DE RESOLUCION DE INFRACTOREN WORD
        //    //Start Word and open the document template.
        //    // Objetos de word
        //    Word.Application oWord = new Word.Application();
        //    Word.Document oDoc;
        //    transferir myTransferencia = new transferir();

        //    object rutaPlantilla = serviciosConfiguraciones.leerRegistroIni("PLANTILLAS") + "\\" + nombrePlantilla;
        //    myTransferencia.archivoDelServerSinAbrir((string)rutaPlantilla);

        //    oWord.Visible = false;

        //    object newTmp = System.Reflection.Missing.Value;
        //    object DocType = false;
        //    object visible = true;

        //    // Ubicación de la plantilla en el disco duro
        //    oDoc = oWord.Documents.Add(ref rutaPlantilla, ref newTmp, ref DocType, ref visible);

        //    // Verifico que la consulta tenga datos
        //    if (myPlantilla.ENCABEZADO != null)
        //    {
        //        object objTmp;
        //        objTmp = "ENCABEZADO";
        //        myPlantilla.ENCABEZADO = myPlantilla.ENCABEZADO.Replace("<t>", "");
        //        oDoc.Bookmarks.get_Item(ref objTmp).Range.Text = myPlantilla.ENCABEZADO.Replace("<->", "");
        //    }
        //    if (myPlantilla.CONTENIDO != null)
        //    {
        //        object objTmp;
        //        objTmp = "CONTENIDO";
        //        myPlantilla.CONTENIDO = myPlantilla.CONTENIDO.Replace("<t>", "");
        //        oDoc.Bookmarks.get_Item(ref objTmp).Range.Text = myPlantilla.CONTENIDO.Replace("<->", "");
        //    }


        //    //SUBIR ARCHIVO AL SERVIDOR
        //    string directorioResoluciones = serviciosConfiguraciones.leerRegistroIni("RESOLUCIONES");
        //    string nombreDocumentoExtension = nombreDocumento + ".doc";
        //    object rutaResolucion = directorioResoluciones + "\\" + nombreDocumentoExtension;

        //    //Specifying the format in which you want the output file 
        //    object format = Word.WdSaveFormat.wdFormatDocument;

        //    // Use for the parameter whose type are not known or  
        //    // say Missing
        //    object Unknown = Type.Missing;

        //    //Changing the format of the document
        //    oWord.ActiveDocument.SaveAs(ref rutaResolucion, ref format,
        //            ref Unknown, ref Unknown, ref Unknown,
        //            ref Unknown, ref Unknown, ref Unknown,
        //            ref Unknown, ref Unknown, ref Unknown,
        //            ref Unknown, ref Unknown, ref Unknown,
        //            ref Unknown, ref Unknown);

        //    //for closing the application
        //    oWord.Quit(ref Unknown, ref Unknown, ref Unknown);

        //    //Tiempo para que word cierre la app
        //    System.Threading.Thread.Sleep(1000);

        //    //Enviar archivo al servidor
        //    myTransferencia.archivoAlServer(rutaResolucion.ToString(), rutaResolucion.ToString());

        //    //Abrir archivo
        //    //System.Diagnostics.Process.Start(rutaResolucion.ToString());
        //}

        private bool validarDatos()
        {
            if (cmbPlantillaImposicion.SelectedIndex < 0)
            {
                MessageBox.Show("Debe seleccionar una plantilla para imposición.");
                cmbPlantillaImposicion.Focus();
                return false;
            }

            if (cmbPlantillaAlcoholemia.SelectedIndex < 0)
            {
                MessageBox.Show("Debe seleccionar una plantilla para alcoholemia.");
                cmbPlantillaAlcoholemia.Focus();
                return false;
            }

            if (cmbPlantillaMotoTaxi.SelectedIndex < 0)
            {
                MessageBox.Show("Debe seleccionar una plantilla para mototaxismo.");
                cmbPlantillaMotoTaxi.Focus();
                return false;
            }

            if (!rbtEspecificarAccion.Checked && !rbtPorDefecto.Checked)
            {
                MessageBox.Show("Debe especificar la acción en caso de alcoholemia.");
                grbAccionAlcoholemia.Focus();
                return false;
            }

            if (rbtEspecificarAccion.Checked && cmbTipoAccionAlcoholemia.SelectedIndex < 0)
            {
                MessageBox.Show("Debe especificar el tipo de acción.");
                cmbTipoAccionAlcoholemia.Focus();
                return false;
            }

            if (rbtEspecificarAccion.Checked && cmbTipoAccionAlcoholemia.SelectedIndex.Equals((int)AccionAlcoholemia.Suspension) && string.IsNullOrWhiteSpace(txtTiempoSuspension.Text))
            {
                MessageBox.Show("Debe especificar el tiempo de suspensión.");
                txtTiempoSuspension.Focus();
                return false;
            }

            if (string.IsNullOrEmpty(txtNumeroResolucion.Text))
            {
                MessageBox.Show("El campo número de resolución es obligatorio.");
                txtNumeroResolucion.Focus();
                return false;
            }

            return true;
        }

        private void chkSeleccionarTodos_CheckedChanged(object sender, EventArgs e)
        {
            if (chkSeleccionarTodos.Checked)
                dtGrViewComparendos.SelectAll();
            else
                dtGrViewComparendos.ClearSelection();
        }

        private void txtNumeroResolucion_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.KeyCode == Keys.Enter && txtNumeroResolucion.Text != "")
                {
                    if (serviciosDocumentos.validarNumeroResolucionComp(txtNumeroResolucion.Text))
                    {
                        btnGenerarResolucion.Enabled = true;
                        numResolucion = int.Parse(txtNumeroResolucion.Text);
                    }
                    else
                    {
                        MessageBox.Show("El número de resolución ya existe!. Ingrese otro número.");
                        btnGenerarResolucion.Enabled = false;
                        txtNumeroResolucion.Focus();
                        numResolucion = 0;
                    }
                }
            }
            catch (Exception exp)
            {
                log.escribirError(exp.ToString(), this.GetType());
                MessageBox.Show("Error desconocido realizando la funcionalidad! " + exp.Message);
                //Console.WriteLine(exp.Message);
                //Console.WriteLine(exp.StackTrace);
            }
        }

        private void cmbPlantillaImposicion_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (cmbPlantillaImposicion.SelectedIndex > -1)
                {
                    myPlantilla = new plantilla();
                    myPlantilla.ID = (int)cmbPlantillaImposicion.SelectedValue;
                    myPlantilla = (plantilla)((Object[])serviciosDocumentos.buscarPlantillasComp(myPlantilla))[0];

                    myTipoResolucion = new tipoResolucionComp();
                    myTipoResolucion.IDTIPORESOLUCION = myPlantilla.IDTIPORESOLUCION;
                    myTipoResolucion = (tipoResolucionComp)((Object[])serviciosDocumentos.buscarTipoResolucionComp(myTipoResolucion))[0];
                }
            }
            catch (Exception exp)
            {
                log.escribirError(exp.ToString(), this.GetType());
                MessageBox.Show("Error desconocido realizando la funcionalidad! " + exp.Message);
                //Console.WriteLine(exp.Message);
                //Console.WriteLine(exp.StackTrace);
            }
        }

        private void cmbPlantillaAlcoholemia_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (cmbPlantillaAlcoholemia.SelectedIndex > -1)
                {
                    myPlantillaAlcoholemia = new plantilla();
                    myPlantillaAlcoholemia.ID = (int)cmbPlantillaAlcoholemia.SelectedValue;
                    myPlantillaAlcoholemia = (plantilla)((Object[])serviciosDocumentos.buscarPlantillasComp(myPlantillaAlcoholemia))[0];

                    myTipoResolucion = new tipoResolucionComp();
                    myTipoResolucion.IDTIPORESOLUCION = myPlantillaAlcoholemia.IDTIPORESOLUCION;
                    myTipoResolucion = (tipoResolucionComp)((Object[])serviciosDocumentos.buscarTipoResolucionComp(myTipoResolucion))[0];
                }
            }
            catch (Exception exp)
            {
                log.escribirError(exp.ToString(), this.GetType());
                MessageBox.Show("Error desconocido realizando la funcionalidad! " + exp.Message);
                //Console.WriteLine(exp.Message);
                //Console.WriteLine(exp.StackTrace);
            }
        }

        private void cmbPlantillaMotoTaxi_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (cmbPlantillaMotoTaxi.SelectedIndex > -1)
                {
                    myPlantillaMototaxismo = new plantilla();
                    myPlantillaMototaxismo.ID = (int)cmbPlantillaMotoTaxi.SelectedValue;
                    myPlantillaMototaxismo = (plantilla)((Object[])serviciosDocumentos.buscarPlantillasComp(myPlantillaMototaxismo))[0];

                    myTipoResolucion = new tipoResolucionComp();
                    myTipoResolucion.IDTIPORESOLUCION = myPlantillaMototaxismo.IDTIPORESOLUCION;
                    myTipoResolucion = (tipoResolucionComp)((Object[])serviciosDocumentos.buscarTipoResolucionComp(myTipoResolucion))[0];
                }
            }
            catch (Exception exp)
            {
                log.escribirError(exp.ToString(), this.GetType());
                MessageBox.Show("Error desconocido realizando la funcionalidad! " + exp.Message);
                //Console.WriteLine(exp.Message);
                //Console.WriteLine(exp.StackTrace);
            }
        }
        
        private void cmbTipoAccionAlcoholemia_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (cmbTipoAccionAlcoholemia.SelectedIndex.Equals((int)AccionAlcoholemia.Suspension))
                    txtTiempoSuspension.Enabled = true;
                else
                    txtTiempoSuspension.Enabled = false;
            }
            catch (Exception exp)
            {
                log.escribirError(exp.ToString(), this.GetType());
                MessageBox.Show("Error desconocido realizando la funcionalidad! " + exp.Message);
            }
        }

        private void btnBuscarComparendos_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;

            try
            {
                cargarComparendos();
            }
            catch (Exception exp)
            {
                log.escribirError(exp.ToString(), this.GetType());
                MessageBox.Show("Error desconocido realizando la funcionalidad! " + exp.Message);
                //Console.WriteLine(exp.Message);
                //Console.WriteLine(exp.StackTrace);
            }

            this.Cursor = Cursors.Arrow;
        }

        #region Eventos Key Press
            private void txtNumResol_KeyPress(object sender, KeyPressEventArgs e)
            {
                Funciones fnc = new Funciones();
                fnc.esNumero(e);
            }

            private void txtDocumento_KeyPress(object sender, KeyPressEventArgs e)
            {
                Funciones myFun = new Funciones();
                myFun.esNumero(e);
            }    

            private void dtInicio_KeyPress(object sender, KeyPressEventArgs e)
            {
                funciones.lanzarTapConEnter(e);
            }

            private void dtFin_KeyPress(object sender, KeyPressEventArgs e)
            {
                funciones.lanzarTapConEnter(e);
            }

            private void cmbBoxPlantillas_KeyPress(object sender, KeyPressEventArgs e)
            {
                funciones.lanzarTapConEnter(e);
            }

            private void txtNumeroResolucion_KeyPress(object sender, KeyPressEventArgs e)
            {
                funciones.esNumero(e);
            }
        #endregion                        

            private void txtMotivo_KeyPress(object sender, KeyPressEventArgs e)
            {
                funciones.esAlfanumerico(e);
            }

            private void txtConsidJurid_KeyPress(object sender, KeyPressEventArgs e)
            {
                funciones.esAlfanumerico(e);
            }

            private void txtTiempoSuspension_KeyPress(object sender, KeyPressEventArgs e)
            {
                funciones.esNumero(e);
            }

            private void txtTiempoSuspension_KeyPress_1(object sender, KeyPressEventArgs e)
            {
                funciones.esNumero(e);
            }
        
    }
}
