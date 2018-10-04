/*
 * Created by: Syeda Anila Nusrat. 
 * Date: 1st August 2009
 * Time: 2:54 PM 
 */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.IO.Ports;
using MySql.Data.MySqlClient;
using System.Timers;
using Microsoft;
using System.Data.Odbc;



namespace SMSapplication
{
    public partial class SMSapplication : Form
    {
        

        #region Constructor
        public SMSapplication()
        {
            InitializeComponent();
        }
        #endregion

        #region Private Variables
        SerialPort port = new SerialPort();
        clsSMS objclsSMS = new clsSMS();
        ShortMessageCollection objShortMessageCollection = new ShortMessageCollection();
        #endregion

        #region Private Methods

        #region Write StatusBar
        private void WriteStatusBar(string status)
        {
            try
            {
                statusBar1.Text = "Message: " + status;
            }
            catch (Exception ex)
            {
                ErrorLog(ex.Message);

            }
        }
        #endregion

        #endregion


        #region Private Events
     
        private void SMSapplication_Load(object sender, EventArgs e)
        {
          
            try
            {
               #region Display all available COM Ports
                string[] ports = SerialPort.GetPortNames();

                // Add all port names to the combo box:
                foreach (string port in ports)
                {
                    this.cboPortName.Items.Add(port);
                }
                #endregion

                //Remove tab pages
                this.tabSMSapplication.TabPages.Remove(tbSendSMS);
                this.tabSMSapplication.TabPages.Remove(tbReadSMS);
                this.tabSMSapplication.TabPages.Remove(tbDeleteSMS);

                this.btnDisconnect.Enabled = false;
            }
            catch (Exception ex)
            {
                ErrorLog(ex.Message);
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            try
            {
                //Open communication port 
                this.port = objclsSMS.OpenPort(this.cboPortName.Text, Convert.ToInt32(this.cboBaudRate.Text), Convert.ToInt32(this.cboDataBits.Text), Convert.ToInt32(this.txtReadTimeOut.Text), Convert.ToInt32(this.txtWriteTimeOut.Text));

                if (this.port != null)
                {
                    this.gboPortSettings.Enabled = false;

                    //MessageBox.Show("Modem is connected at PORT " + this.cboPortName.Text);
                    this.statusBar1.Text = "Modem is connected at PORT " + this.cboPortName.Text;

                    //Add tab pages
                    this.tabSMSapplication.TabPages.Add(tbSendSMS);
                    this.tabSMSapplication.TabPages.Add(tbReadSMS);
                    this.tabSMSapplication.TabPages.Add(tbDeleteSMS);

                    this.lblConnectionStatus.Text = "Connected at " + this.cboPortName.Text;
                    this.btnDisconnect.Enabled = true;
                }

                else
                {
                    //MessageBox.Show("Invalid port settings");
                    this.statusBar1.Text = "Invalid port settings";
                }
            }
            catch (Exception ex)
            {
                ErrorLog(ex.Message);
            }

        }
        private void btnDisconnect_Click(object sender, EventArgs e)
        {
            try
            {
                this.gboPortSettings.Enabled = true;
                objclsSMS.ClosePort(this.port);

                //Remove tab pages
                this.tabSMSapplication.TabPages.Remove(tbSendSMS);
                this.tabSMSapplication.TabPages.Remove(tbReadSMS);
                this.tabSMSapplication.TabPages.Remove(tbDeleteSMS);

                this.lblConnectionStatus.Text = "Not Connected";
                this.btnDisconnect.Enabled = false;

            }
            catch (Exception ex)
            {
                ErrorLog(ex.Message);
            }
        }

        public void NotificarClientes()
        {
            var table = new DataSet();

           /* string cnx = @"Server=192.158.237.98; Database=waveslab_mp_db; Uid=waveslab_web; Pwd=rDcS6GwEYKhnw8dt; persistsecurityinfo=True; Port=3306; SslMode=none;";*/

           /*   string cnx = @"Server=192.158.237.98; Database=waveslab_DB; Uid=waveslab_web; Pwd=rDcS6GwEYKhnw8dt; persistsecurityinfo=True; Port=3306; SslMode=none;";*/

           string cnx = @"Server=192.158.237.98; Database=masterzo_db; Uid=masterzon_sms; Pwd=rMD)AO4Kr9O0; persistsecurityinfo=True; Port=3306; SslMode=none;";

            btnSendSMS.Text = "Detener";
            btnSendSMS.BackColor = Color.Coral;

            if (cnx.Substring(32,11) == "waveslab_DB")
            {
                tbSendSMS.BackColor = Color.AntiqueWhite;
            }
            try
            {
                 
                using (MySqlConnection conn = new MySqlConnection(cnx))
                {
                    string s = "SELECT num_destinatario,des_mensaje,cod_notificacion FROM sis_notificaciones_sms where cod_notificacion = (SELECT min(cod_notificacion) as cod_notificacion FROM sis_notificaciones_sms WHERE ind_prioridad = (select min(ind_prioridad) as ind_prioridad FROM sis_notificaciones_sms where cod_estado = 'P') and cod_estado = 'P')";
                    
                    using (conn)
                    using (MySqlCommand mcd2 = new MySqlCommand(s, conn))
                    {
                        conn.Open();
                        using (MySqlDataReader mdr = mcd2.ExecuteReader())
                        {
                            DataTable dt = new DataTable("Tabla_Pendientes");
                            dt.Load(mdr);
                            table.Tables.Add(dt);

                            DateTime hora = DateTime.Now;
                            txtFecHoraInicio.Text = hora.ToString("yyyy/MM/dd_HH:mm:ss");
                            this.textIntervalos.Text = "120";
                            foreach (DataRow fila in dt.Rows)
                            {

                                this.textIntervalos.Text = "20";

                                string num_destino = fila["num_destinatario"].ToString();
                                string txt_mensaje = fila["des_mensaje"].ToString();
                                string codigo = fila["cod_notificacion"].ToString();

                                /*----------------------------------------------*/
                                string sql_update = string.Concat("UPDATE sis_notificaciones_sms SET cod_estado = 'E', fec_hora_envio = now() WHERE cod_notificacion = ", codigo);
                                MySqlCommand mcd3 = new MySqlCommand(sql_update, conn);
                                MySqlDataReader mdrUpdate = mcd3.ExecuteReader();

                                if (objclsSMS.sendMsg(this.port, num_destino, txt_mensaje))
                                {
                                  /*  string sql_update = string.Concat("UPDATE sis_notificaciones_sms SET cod_estado = 'E', fec_hora_envio = now() WHERE cod_notificacion = ", codigo );
                                    MySqlCommand mcd3 = new MySqlCommand(sql_update, conn);
                                    MySqlDataReader mdrUpdate = mcd3.ExecuteReader();
                                    */

                                    //MessageBox.Show("Message has sent successfully");
                                    this.statusBar1.Text = string.Concat("Enviado correctamente al numero: ", num_destino) ;
                                    this.txtMessage.Text = string.Concat(txtFecHoraInicio.Text," Mensaje enviado correctamente al numero: ", num_destino) + Environment.NewLine + this.txtMessage.Text;

                                }
                                else
                                {
                                    //MessageBox.Show("Failed to send message");
                                    this.statusBar1.Text = string.Concat("ERROR en envio de SMS al numero: ", num_destino);
                                    this.txtMessage.Text = string.Concat("ERROR en envio de SMS al numero: ", num_destino) + Environment.NewLine  + this.txtMessage.Text;

                                    string sql_bitacora = string.Concat("INSERT INTO sis_bitacora_eventos(`fec_hora_evento`, `cod_usuario`, `des_archivo`, `des_proceso`, `des_query`, `des_definicion`) VALUES(DATE_ADD(now(), INTERVAL 2 HOUR), 'Mensajero', 'SMSapplication', 'NotificarClientes()',", s ,", 'ERROR en envio de SMS')");
                                    
                                    MySqlCommand mcd_bitacora = new MySqlCommand(sql_bitacora, conn);
                                    MySqlDataReader mdrBitacora = mcd_bitacora.ExecuteReader();

                                }

                            }
                                
                            }
                    }
                   
                }               
            }

            catch (Exception ex)
            {
                this.txtMessage.Text = this.txtMessage.Text + Environment.NewLine + ex.Message + txtFecHoraInicio.Text;
                ErrorLog(ex.Message);
            }
        }

        void timer_Tick(object sender, EventArgs e)
        {
            NotificarClientes();
        }

        private void btnSendSMS_Click(object sender, EventArgs e)
        {
            if (btnSendSMS.Text == "Detener")
            {
                this.gboPortSettings.Enabled = true;
                objclsSMS.ClosePort(this.port);

                //Remove tab pages
                this.tabSMSapplication.TabPages.Remove(tbSendSMS);
                this.tabSMSapplication.TabPages.Remove(tbReadSMS);
                this.tabSMSapplication.TabPages.Remove(tbDeleteSMS);

                this.lblConnectionStatus.Text = "Not Connected";
                this.btnDisconnect.Enabled = false;

                btnSendSMS.Text = "Iniciar";
                btnSendSMS.BackColor = Color.DodgerBlue;
            }

            System.Windows.Forms.Timer t = new System.Windows.Forms.Timer();
            //.............................................. Send SMS ....................................................
            try
            {
                t.Interval = Int32.Parse(textIntervalos.Text) * 1000; // specify interval time as you want
                if (t.Interval < 20) {
                    t.Interval = 20;
                }
                t.Tick += new EventHandler(timer_Tick);
                t.Start();
                
            }
            catch (Exception ex)
            {
                ErrorLog(ex.Message);
            }
        }
        private void btnReadSMS_Click(object sender, EventArgs e)
        {
            try
            {
                //count SMS 
                int uCountSMS = objclsSMS.CountSMSmessages(this.port);
                if (uCountSMS > 0)
                {

                    #region Command
                    string strCommand = "AT+CMGL=\"ALL\"";

                    if (this.rbReadAll.Checked)
                    {
                        strCommand = "AT+CMGL=\"ALL\"";
                    }
                    else if (this.rbReadUnRead.Checked)
                    {
                        strCommand = "AT+CMGL=\"REC UNREAD\"";
                    }
                    else if (this.rbReadStoreSent.Checked)
                    {
                        strCommand = "AT+CMGL=\"STO SENT\"";
                    }
                    else if (this.rbReadStoreUnSent.Checked)
                    {
                        strCommand = "AT+CMGL=\"STO UNSENT\"";
                    }
                    #endregion

                    // If SMS exist then read SMS
                    #region Read SMS
                    //.............................................. Read all SMS ....................................................
                    objShortMessageCollection = objclsSMS.ReadSMS(this.port, strCommand);
                    foreach (ShortMessage msg in objShortMessageCollection)
                    {

                        ListViewItem item = new ListViewItem(new string[] { msg.Index, msg.Sent, msg.Sender, msg.Message });
                        item.Tag = msg;
                        lvwMessages.Items.Add(item);

                    }
                    #endregion

                }
                else
                {
                    lvwMessages.Clear();
                    //MessageBox.Show("There is no message in SIM");
                    this.statusBar1.Text = "There is no message in SIM";
                    
                }
            }
            catch (Exception ex)
            {
                ErrorLog(ex.Message);
            }
        }
        private void btnDeleteSMS_Click(object sender, EventArgs e)
        {
            try
            {
                //Count SMS 
                int uCountSMS = objclsSMS.CountSMSmessages(this.port);
                if (uCountSMS > 0)
                {
                    DialogResult dr = MessageBox.Show("Are u sure u want to delete the SMS?", "Delete confirmation", MessageBoxButtons.YesNo);

                    if (dr.ToString() == "Yes")
                    {
                        #region Delete SMS

                        if (this.rbDeleteAllSMS.Checked)
                        {                           
                            //...............................................Delete all SMS ....................................................

                            #region Delete all SMS
                            string strCommand = "AT+CMGD=1,4";
                            if (objclsSMS.DeleteMsg(this.port, strCommand))
                            {
                                //MessageBox.Show("Messages has deleted successfuly ");
                                this.statusBar1.Text = "Messages has deleted successfuly";
                            }
                            else
                            {
                                //MessageBox.Show("Failed to delete messages ");
                                this.statusBar1.Text = "Failed to delete messages";
                            }
                            #endregion
                            
                        }
                        else if (this.rbDeleteReadSMS.Checked)
                        {                          
                            //...............................................Delete Read SMS ....................................................

                            #region Delete Read SMS
                            string strCommand = "AT+CMGD=1,3";
                            if (objclsSMS.DeleteMsg(this.port, strCommand))
                            {
                                //MessageBox.Show("Messages has deleted successfuly");
                                this.statusBar1.Text = "Messages has deleted successfuly";
                            }
                            else
                            {
                                //MessageBox.Show("Failed to delete messages ");
                                this.statusBar1.Text = "Failed to delete messages";
                            }
                            #endregion

                        }

                        #endregion
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorLog(ex.Message);
            }

        }
        private void btnCountSMS_Click(object sender, EventArgs e)
        {
            try
            {
                //Count SMS
                int uCountSMS = objclsSMS.CountSMSmessages(this.port);
                this.txtCountSMS.Text = uCountSMS.ToString();
            }
            catch (Exception ex)
            {
                ErrorLog(ex.Message);
            }
        }

        #endregion

        #region Error Log
        public void ErrorLog(string Message)
        {
            StreamWriter sw = null;

            try
            {
                WriteStatusBar(Message);

                string sLogFormat = DateTime.Now.ToShortDateString().ToString() + " " + DateTime.Now.ToLongTimeString().ToString() + " ==> ";
                //string sPathName = @"E:\";
                string sPathName = @"SMSapplicationErrorLog_";

                string sYear = DateTime.Now.Year.ToString();
                string sMonth = DateTime.Now.Month.ToString();
                string sDay = DateTime.Now.Day.ToString();

                string sErrorTime = sDay + "-" + sMonth + "-" + sYear;

                sw = new StreamWriter(sPathName + sErrorTime + ".txt", true);

                sw.WriteLine(sLogFormat + Message);
                sw.Flush();

            }
            catch (Exception ex)
            {
                //ErrorLog(ex.ToString());
            }
            finally
            {
                if (sw != null)
                {
                    sw.Dispose();
                    sw.Close();
                }
            }
            
        }
        #endregion

       
    }
}