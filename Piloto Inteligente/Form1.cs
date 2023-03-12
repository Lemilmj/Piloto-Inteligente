using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Configuration;

namespace Piloto_Inteligente
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        int WeightUnit = 1;
        double MasaVehiculo = 0;
        double MasaCarga = 0;
        double[] PesoNetoVehiculo = { 0, 0 };
        double[] PesoNetoCarga = { 0, 0 };
        double[] Pesototal = { 0, 0 };
        double DistanciaDesp = 0;
        double TiempoDesp = 0;
        double VelMaxAlc = 0;
        double accMaxAlc = 0;
        double angInclCalle = 0.1;
        double fuerzaAtracc = 0;
        double fuerzaNeta = 0;
        double fuerzaReqImp = 0;
        double ConsumoPR = 0;
        double MaxRpm = 0;
        double PotMotor = 0;
        double RevReq = 0;
        double PotReq = 0;
        double CombReq = 0;
        double NivelCombust = 0;
        int errCount = 0;
        int state = 0;


       private void btnEmpezar_Click(object sender, EventArgs e)
        {
            gbDetalle.Visible = false;
            errCount = 0;
            richDetalle.Text = "";
            var Registro = gbVehicle.Controls.OfType<NumericUpDown>().ToList();
            Registro.AddRange(gbParameters.Controls.OfType<NumericUpDown>().ToList());
            var Campos = gbVehicle.Controls.OfType<Label>().ToList();
            Campos.AddRange(gbParameters.Controls.OfType<Label>().ToList());
            Campos = (from C in Campos
                    join R in Registro
                    on C.Name.Remove(0, 3) 
                    equals R.Name.Remove(0, 2) select C).ToList();
            var RegistrosVacios = (from C in Campos
                                   join R in Registro
                                   on C.Name.Remove(0, 3)
                                   equals R.Name.Remove(0, 2)
                                   where R.Value == 0
                                   select C).ToList();
            foreach (Label campo in Campos)
            {
                campo.ForeColor = DefaultForeColor;
                campo.BackColor = DefaultBackColor;
            }
            switch (state)
            {
                case 0:
                    if (Registro.Where(x => x.Value == 0).Count() == Registro.Count())
                    {
                        AddError(Campos, new string[] { "Todos los Valores de entrada están en cero" });
                    }
                    else if (RegistrosVacios.Count() > 0)
                    {
                        foreach (Label label in RegistrosVacios)
                        {
                            AddError(label, $"El Valor del/la {label.Text} es cero.");
                        }
                    }
                    if (NivelCombust <= CombReq)
                    {
                        AddError(lblNivelComb, "El Nivel de Combustible es menor que el requerido");
                    }

                    if (PotMotor <= PotReq)
                    {
                        AddError(lblPotMaxMot, "La Potencia del motor es menor que la requerida");
                    }

                    if (errCount == 0)
                    {
                        btnEmpezar.Text = "Detener";
                        btnEmpezar.BackColor = Color.Green;
                        state = 1;
                        MessageBox.Show("The process is runin", "Notification", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    break;
                case 1:
                    btnEmpezar.Text = "Empezar";
                    btnEmpezar.BackColor = Color.Blue;
                    state = 0;
                    break;
            }
        }
       public void AddError(Label label, string Detalle)
        {
            richDetalle.Text = richDetalle.Text == "" ? "- " + Detalle : $"{richDetalle.Text}\n- {Detalle}";
            label.BackColor = Color.Red;
            label.ForeColor = Color.White;
            errCount++;
            gbDetalle.Visible = true;
        }

        public void AddError (List<Label> labelList, string[] Detalles)
        {
            int idx = 0;
            foreach (Label label in labelList)
            {
                if (idx < Detalles.Length)
                richDetalle.Text = richDetalle.Text == "" ? "- " + Detalles[idx] : $"{richDetalle.Text}\n- {Detalles[idx]}";
                label.BackColor = Color.Red;
                label.ForeColor = Color.White;
                idx++;
            }
            gbDetalle.Visible = true;
        }
        private void nmMasaVehiculo_ValueChanged(object sender, EventArgs e)
        {
            MasaVehiculo = Convert.ToDouble(nmMasaVehiculo.Value);
            PesoNetoVehiculo[0] = Convert.ToDouble(MasaVehiculo) * Unidades.g;
            PesoNetoVehiculo[1] = Unidades.NewtontoPound(PesoNetoVehiculo[0]);
            Pesototal[0] = PesoNetoVehiculo[0] + PesoNetoCarga[0];
            Pesototal[1] = PesoNetoVehiculo[1] + PesoNetoCarga[1];

            switch (WeightUnit)
            {
                case 0:
                    resPesoNetoVehic.Text = PesoNetoVehiculo[1] == 0 ? "0" : Math.Round(PesoNetoVehiculo[1], 4).ToString() + " Libras";
                    resPesoTot.Text = Pesototal[1] == 0 ? "0" : (Math.Round(Pesototal[1], 4)).ToString() + " Libras";
                    break;
                case 1:
                    resPesoNetoVehic.Text = PesoNetoVehiculo[0] == 0 ? "0" : Math.Round(PesoNetoVehiculo[0], 4).ToString() + " Newtons";
                    resPesoTot.Text = Pesototal[0] == 0 ? "0" : Math.Round(Pesototal[0], 4).ToString() + " Newtons";
                    break;
            }
            fuerzaAtracc = Pesototal[0] * Math.Abs(Math.Sin(angInclCalle));
            fuerzaNeta = Unidades.FuerzaNeta(accMaxAlc, angInclCalle, MasaVehiculo, MasaCarga);
            fuerzaReqImp = fuerzaAtracc + fuerzaNeta;
            PotReq = Unidades.CalcPot(fuerzaReqImp, DistanciaDesp, TiempoDesp);
            RevReq = Unidades.RevReq(MaxRpm, PotMotor, PotReq);
            CombReq = Unidades.CombReq(RevReq, ConsumoPR, NivelCombust, TiempoDesp)[0];
            resFuerzaReqImp.Text = Math.Round(fuerzaReqImp).ToString() + " Newtons";
            resFuerzaNeta.Text = Math.Round(fuerzaNeta, 4).ToString() + " Newtons";
            resFuerzaAtrac.Text = Math.Round(fuerzaAtracc, 4).ToString() + " Newtons";
            resPotReq.Text = Math.Round(PotReq, 4).ToString() + " hp";
            resRevReq.Text = Math.Round(RevReq, 4).ToString() + " RPM";
            resComReq.Text = Math.Round(CombReq, 3).ToString() + " Litros";
            lblMasaVehiculo.BackColor = DefaultBackColor;
            lblMasaVehiculo.ForeColor = DefaultForeColor;
        }

        private void nmRpmMotor_ValueChanged(object sender, EventArgs e)
        {
            MaxRpm = Convert.ToDouble(nmMaxrpm.Value);
            RevReq = Unidades.RevReq(MaxRpm, PotMotor, PotReq);
            CombReq = Unidades.CombReq(RevReq, ConsumoPR, NivelCombust, TiempoDesp)[0];
            resRevReq.Text = Math.Round(RevReq, 4).ToString() + " RPM";
            resComReq.Text = Math.Round(CombReq, 4)
                .ToString() + " Litros";
            lblMaxrpm.BackColor = DefaultBackColor;
            lblMaxrpm.ForeColor = DefaultForeColor;
        }

        private void nmPotMaxMot_ValueChanged(object sender, EventArgs e)
        {
            PotMotor = Convert.ToDouble(nmPotMaxMot.Value);
            RevReq = Unidades.RevReq(MaxRpm, PotMotor, PotReq);
            CombReq = Unidades.CombReq(RevReq, ConsumoPR, NivelCombust, TiempoDesp)[0];
            resRevReq.Text = Math.Round(RevReq, 4).ToString() + " RPM";
            resComReq.Text = Math.Round(CombReq, 4)
                .ToString() + " Litros";
            lblPotMaxMot.BackColor = DefaultBackColor;
            lblPotMaxMot.ForeColor = DefaultForeColor;
        }

        private void nmConsumoRev_ValueChanged(object sender, EventArgs e)
        {
            ConsumoPR = Convert.ToDouble(nmConsumoRev.Value);
            CombReq = Unidades.CombReq(RevReq, ConsumoPR, NivelCombust, TiempoDesp)[0];
            resComReq.Text = Math.Round(CombReq, 4)
                .ToString() + " Litros";
            lblConsumoRev.BackColor = DefaultBackColor;
            lblConsumoRev.ForeColor = DefaultForeColor;
        }

        private void nmNivelComb_ValueChanged(object sender, EventArgs e)
        {
            NivelCombust = Convert.ToDouble(nmNivelComb.Value);
            CombReq = Unidades.CombReq(RevReq, ConsumoPR, NivelCombust, TiempoDesp)[0];
            resComReq.Text = Math.Round(CombReq, 4)
                .ToString() + " Litros";
            lblNivelComb.BackColor = DefaultBackColor;
            lblNivelComb.ForeColor = DefaultForeColor;
        }

        private void nmMasaCarga_ValueChanged(object sender, EventArgs e)
        {
            MasaCarga = Convert.ToDouble(nmMasaCarga.Value);
            PesoNetoCarga[0] = Convert.ToDouble(MasaCarga) * Unidades.g;
            PesoNetoCarga[1] = Unidades.NewtontoPound(PesoNetoCarga[0]);
            Pesototal[0] = PesoNetoVehiculo[0] + PesoNetoCarga[0];
            Pesototal[1] = PesoNetoVehiculo[1] + PesoNetoCarga[1];

            switch (WeightUnit)
            {
                case 0:
                    resPesoCarga.Text = PesoNetoCarga[1] == 0 ? "0" : Math.Round(PesoNetoCarga[1], 4).ToString() + " Libras";
                    resPesoTot.Text = Pesototal[1] == 0 ? "0" : Math.Round(Pesototal[1], 4).ToString() + " Libras";
                    break;
                case 1:
                    resPesoCarga.Text = PesoNetoCarga[0] == 0 ? "0" : Math.Round(PesoNetoCarga[0], 4).ToString() + " Newtons";
                    resPesoTot.Text = Pesototal[0] == 0 ? "0" : Math.Round(Pesototal[0], 4).ToString() + " Newtons";
                    break;
            }
            fuerzaAtracc = Pesototal[0] * Math.Abs(Math.Sin(angInclCalle));
            fuerzaNeta = Unidades.FuerzaNeta(accMaxAlc, angInclCalle, MasaVehiculo, MasaCarga);
            fuerzaReqImp = fuerzaAtracc + fuerzaNeta;
            PotReq = Unidades.CalcPot(fuerzaReqImp, DistanciaDesp, TiempoDesp);
            RevReq = Unidades.RevReq(MaxRpm, PotMotor, PotReq);
            CombReq = Unidades.CombReq(RevReq, ConsumoPR, NivelCombust, TiempoDesp)[0];
            resFuerzaReqImp.Text = Math.Round(fuerzaReqImp).ToString() + " Newtons";
            resFuerzaNeta.Text = Math.Round(fuerzaNeta, 4).ToString() + " Newtons";
            resFuerzaAtrac.Text = Math.Round(fuerzaAtracc, 4).ToString() + " Newtons";
            resPotReq.Text = Math.Round(PotReq, 4).ToString() + " hp";
            resRevReq.Text = Math.Round(RevReq, 4).ToString() + " RPM";
            resComReq.Text = Math.Round(CombReq, 3).ToString() + " Litros";
            lblMasaCarga.BackColor = DefaultBackColor;
            lblMasaCarga.ForeColor = DefaultForeColor;
        }

        private void nmAnguloInc_ValueChanged(object sender, EventArgs e)
        {
            angInclCalle = Convert.ToDouble(nmAnguloInc.Value);
            fuerzaAtracc = Pesototal[0] * Math.Abs(Math.Sin(angInclCalle));
            fuerzaNeta = Unidades.FuerzaNeta(accMaxAlc, angInclCalle, MasaVehiculo, MasaCarga);
            fuerzaReqImp = fuerzaAtracc + fuerzaNeta;
            PotReq = Unidades.CalcPot(fuerzaReqImp, DistanciaDesp, TiempoDesp);
            RevReq = Unidades.RevReq(MaxRpm, PotMotor, PotReq);
            CombReq = Unidades.CombReq(RevReq, ConsumoPR, NivelCombust, TiempoDesp)[0];
            resFuerzaReqImp.Text = Math.Round(fuerzaReqImp).ToString() + " Newtons";
            resFuerzaAtrac.Text = Math.Round(fuerzaAtracc, 4).ToString() + " Newtons";
            resFuerzaNeta.Text = Math.Round(fuerzaNeta, 4).ToString() + " Newtons";
            resPotReq.Text = Math.Round(PotReq, 4).ToString() + " hp";
            resRevReq.Text = Math.Round(RevReq, 4).ToString() + " RPM";
            resComReq.Text = Math.Round(CombReq, 3).ToString() + " Litros";
            lblAnguloInc.BackColor = DefaultBackColor;
            lblAnguloInc.ForeColor = DefaultForeColor;
        }
        private void nmTiempoCompDes_ValueChanged(object sender, EventArgs e)
        {
            TiempoDesp = Convert.ToDouble(nmTiempoCompDes.Value);
            if (nmDistaciaDesp.Value == 0)
            {
                resVelMaxAlc.Text = "0";
                resAccMaxAlc.Text = "0";
            }
            VelMaxAlc = DistanciaDesp / TiempoDesp;
            accMaxAlc = VelMaxAlc / TiempoDesp;
            fuerzaNeta = Unidades.FuerzaNeta(accMaxAlc, angInclCalle, MasaVehiculo, MasaCarga);
            fuerzaReqImp = fuerzaAtracc + fuerzaNeta;
            PotReq = Unidades.CalcPot(fuerzaReqImp, DistanciaDesp, TiempoDesp);
            RevReq = Unidades.RevReq(MaxRpm, PotMotor, PotReq);
            CombReq = Unidades.CombReq(RevReq, ConsumoPR, NivelCombust, TiempoDesp)[0];
            resFuerzaReqImp.Text = Math.Round(fuerzaReqImp).ToString() + " Newtons";
            resVelMaxAlc.Text = Math.Round(VelMaxAlc, 4).ToString() + " mts/s";
            resAccMaxAlc.Text = Math.Round(accMaxAlc, 5).ToString() + " mts/s²";
            resFuerzaNeta.Text = Math.Round(fuerzaNeta, 4).ToString() + " Newtons";
            resPotReq.Text = Math.Round(PotReq, 4).ToString() + " hp";
            resRevReq.Text = Math.Round(RevReq, 4).ToString() + " RPM";
            resComReq.Text = Math.Round(CombReq, 3).ToString() + " Litros";
            lblTiempoCompDes.BackColor = DefaultBackColor;
            lblTiempoCompDes.ForeColor = DefaultForeColor;
        }

        private void nmDistaciaDesp_ValueChanged(object sender, EventArgs e)
        {
            if (nmDistaciaDesp.Value == 0)
            {
                resVelMaxAlc.Text = "0";
                resAccMaxAlc.Text = "0";
            }
            DistanciaDesp = Convert.ToDouble(nmDistaciaDesp.Value);
            VelMaxAlc = DistanciaDesp / TiempoDesp;
            accMaxAlc = VelMaxAlc / TiempoDesp;
            fuerzaNeta = Unidades.FuerzaNeta(accMaxAlc, angInclCalle, MasaVehiculo, MasaCarga);
            fuerzaReqImp = fuerzaAtracc + fuerzaNeta;
            PotReq = Unidades.CalcPot(fuerzaReqImp, DistanciaDesp, TiempoDesp);
            RevReq = Unidades.RevReq(MaxRpm, PotMotor, PotReq);
            CombReq = Unidades.CombReq(RevReq, ConsumoPR, NivelCombust, TiempoDesp)[0];
            resFuerzaReqImp.Text = Math.Round(fuerzaReqImp).ToString() + " Newtons";
            resVelMaxAlc.Text = Math.Round(VelMaxAlc, 4).ToString() + " mts/s";
            resAccMaxAlc.Text = Math.Round(accMaxAlc, 5).ToString() + " mts/s²";
            resFuerzaNeta.Text = Math.Round(fuerzaNeta, 4).ToString() + " Newtons";
            resPotReq.Text = Math.Round(PotReq, 4).ToString() + " hp";
            resRevReq.Text = Math.Round(RevReq, 4).ToString() + " RPM";
            resComReq.Text = Math.Round(CombReq, 3).ToString() + " Litros";
            lblDistaciaDesp.BackColor = DefaultBackColor;
            lblDistaciaDesp.ForeColor = DefaultForeColor;
        }

        private void librToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (WeightUnit != 0)
            {
                msPeso.DropDownItems[WeightUnit].Text = msPeso.DropDownItems[WeightUnit].Text.Remove(0, 2);
                msLibras.Text = "✓ " + msLibras.Text;
                resPesoNetoVehic.Text = PesoNetoVehiculo[0] == 0 ? "0" : PesoNetoVehiculo[1].ToString() + " Libras";
                resPesoCarga.Text = PesoNetoCarga[0] == 0 ? "0" : PesoNetoVehiculo[1].ToString() + " Libras";
                resPesoTot.Text = Pesototal[0] == 0 ? "0" : Pesototal[1].ToString() + " Libras";
                WeightUnit = 0;
            }
            return;
        }

        private void newtonToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (WeightUnit != 1)
            {
                msPeso.DropDownItems[WeightUnit].Text = msPeso.DropDownItems[WeightUnit].Text.Remove(0, 2);
                msNewtons.Text = "✓ " + msNewtons.Text;
                resPesoNetoVehic.Text = PesoNetoVehiculo[1] == 0 ? "0" : PesoNetoVehiculo[0].ToString() + " Newtons";
                resPesoCarga.Text = PesoNetoCarga[1] == 0 ? "0" : PesoNetoVehiculo[0].ToString() + " Newtons";
                resPesoTot.Text = Pesototal[1] == 0 ? "0" : Pesototal[0].ToString() + " Newtons";
                WeightUnit = 1;
            }
            return;
        }

        private void fijarValoresDePruebaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            nmMaxrpm.Value = 7000;
            nmPotMaxMot.Value = 600;
            nmNivelComb.Value = 300;
            nmMasaVehiculo.Value = 10000;
            nmMasaCarga.Value = 5000;
            nmDistaciaDesp.Value = 40000;
            nmConsumoRev.Value = 3;
            nmAnguloInc.Value = (decimal)0.1;
            nmTiempoCompDes.Value = 60000;
            richDetalle.Text = "";
            gbDetalle.Visible = false;
        }

        private void btnReiniciar_Click(object sender, EventArgs e)
        {
            btnEmpezar.BackColor = Color.Blue;
            btnEmpezar.Text = "Empezar";

            var Registro = gbVehicle.Controls.OfType<NumericUpDown>().ToList();
            Registro.AddRange(gbParameters.Controls.OfType<NumericUpDown>().ToList());
            var Campos = gbVehicle.Controls.OfType<Label>().ToList();
            Campos.AddRange(gbParameters.Controls.OfType<Label>().ToList());
            Campos = (from C in Campos
                      join R in Registro
                      on C.Name.Remove(0, 3)
                      equals R.Name.Remove(0, 2)
                      select C).ToList();
            var Results = gbResult.Controls.OfType<Label>().ToList();
            Results = Results.Where(x => x.Name.Remove(3, x.Name.Length - 3) == "res").ToList();
            foreach (Label label in Campos)
            {
                label.BackColor = DefaultBackColor;
                label.ForeColor = DefaultForeColor;
            }
            foreach (NumericUpDown valor in Registro)
            {
                valor.Value = 0;
            }
            foreach (Label label in Results)
            {
                label.Text = "0";
            }

            richDetalle.Text = "";
            gbDetalle.Visible = false;
            WeightUnit = 1;
            MasaVehiculo = 0;
            MasaCarga = 0;
            PesoNetoVehiculo = new double[] { 0, 0 };
            PesoNetoCarga = new double[] { 0, 0 };
            Pesototal = new double[] { 0, 0 };
            DistanciaDesp = 0;
            TiempoDesp = 0;
            VelMaxAlc = 0;
            accMaxAlc = 0;
            angInclCalle = 0.1;
            fuerzaAtracc = 0;
            fuerzaNeta = 0;
            fuerzaReqImp = 0;
            ConsumoPR = 0;
            MaxRpm = 0;
            PotMotor = 0;
            RevReq = 0;
            PotReq = 0;
            CombReq = 0;
            NivelCombust = 0;
            errCount = 0;
            state = 0;

        }
    }
}
