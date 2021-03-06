﻿/********************************************************
 * Module Name    : VFramwork
 * Purpose        : Business Partner Model
 * Class Used     : X_C_BPartner
 * Chronological Development
 * Veena Pandey     02-June-2009
 * Raghunandan      24-june-2009
 ******************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VAdvantage.Classes;
using VAdvantage.Common;
using VAdvantage.Process;
using System.Windows.Forms;
using VAdvantage.Model;
using VAdvantage.DataBase;
using VAdvantage.SqlExec;
using VAdvantage.Utility;
using System.Data;
using System.Data.SqlClient;
using VAdvantage.Logging;

namespace VAdvantage.Model
{
    /// <summary>
    /// Business Partner Model
    /// </summary>
    public class MBPartner : X_C_BPartner
    {
        #region private Variables
        // Users						
        private MUser[] _contacts = null;
        //Addressed						
        private MBPartnerLocation[] _locations = null;
        // BP Bank Accounts				
        private MBPBankAccount[] _accounts = null;
        // Prim Address					
        private int? _primaryC_BPartner_Location_ID = null;
        // Prim User						
        private int? _primaryAD_User_ID = null;
        // Credit Limit recently calcualted		
        private bool _TotalOpenBalanceSet = false;
        // BP Group						
        private MBPGroup _group = null;
        private static VLogger _log = VLogger.GetVLogger(typeof(MBPartner).FullName);
        #endregion

        /// <summary>
        /// Get Empty Template Business Partner
        /// </summary>
        /// <param name="ctx">context</param>
        /// <param name="AD_Client_ID">client</param>
        /// <returns>Template Business Partner or null</returns>
        public static MBPartner GetTemplate(Ctx ctx, int AD_Client_ID)
        {
            MBPartner template = GetBPartnerCashTrx(ctx, AD_Client_ID);
            if (template == null)
                template = new MBPartner(ctx, 0, null);
            //	Reset
            if (template != null)
            {
                template.Set_ValueNoCheck("C_BPartner_ID", 0);
                template.SetValue("");
                template.SetName("");
                template.SetName2(null);
                template.SetDUNS("");
                template.SetFirstSale(null);
                //
                template.SetSO_CreditLimit(Env.ZERO);
                template.SetSO_CreditUsed(Env.ZERO);
                template.SetTotalOpenBalance(Env.ZERO);
                //	s_template.setRating(null);
                //
                template.SetActualLifeTimeValue(Env.ZERO);
                template.SetPotentialLifeTimeValue(Env.ZERO);
                template.SetAcqusitionCost(Env.ZERO);
                template.SetShareOfCustomer(0);
                template.SetSalesVolume(0);
            }
            return template;
        }

        /// <summary>
        /// Get Cash Trx Business Partner
        /// </summary>
        /// <param name="ctx">context</param>
        /// <param name="AD_Client_ID">client</param>
        /// <returns>Cash Trx Business Partner or null</returns>
        public static MBPartner GetBPartnerCashTrx(Ctx ctx, int AD_Client_ID)
        {
            MBPartner retValue = null;
            String sql = "SELECT * FROM C_BPartner "
                + " WHERE C_BPartner_ID IN (SELECT C_BPartnerCashTrx_ID FROM AD_ClientInfo" +
                " WHERE AD_Client_ID=" + AD_Client_ID + ")";
            DataTable dt = null;
            IDataReader idr = null;
            try
            {
                idr = DataBase.DB.ExecuteReader(sql, null, null);
                dt = new DataTable();
                dt.Load(idr);
                idr.Close();
                foreach (DataRow dr in dt.Rows)
                {
                    retValue = new MBPartner(ctx, dr, null);
                }
                if (dt == null)
                {
                    _log.Log(Level.SEVERE, "Not found for AD_Client_ID=" + AD_Client_ID);
                }
            }
            catch (Exception e)
            {
                if (idr != null)
                {
                    idr.Close();
                }
                _log.Log(Level.SEVERE, sql, e);
            }
            finally { dt = null; }
            return retValue;
        }

        /// <summary>
        /// Get BPartner with Value
        /// </summary>
        /// <param name="ctx">context</param>
        /// <param name="Value">value</param>
        /// <returns>BPartner or null</returns>
        public static MBPartner Get(Ctx ctx, String Value)
        {
            if (Value == null || Value.Length == 0)
                return null;
            MBPartner retValue = null;
            int AD_Client_ID = ctx.GetAD_Client_ID();
            String sql = "SELECT * FROM C_BPartner WHERE Value=@Value"
                + " AND AD_Client_ID=" + AD_Client_ID;
            DataTable dt = null;
            IDataReader idr = null;
            try
            {
                SqlParameter[] param = new SqlParameter[1];
                param[0] = new SqlParameter("@Value", Value);
                idr = DataBase.DB.ExecuteReader(sql, param, null);
                dt = new DataTable();
                dt.Load(idr);
                idr.Close();
                foreach (DataRow dr in dt.Rows)
                {
                    retValue = new MBPartner(ctx, dr, null);
                }
            }
            catch (Exception e)
            {
                if (idr != null)
                {
                    idr.Close();
                }
                _log.Log(Level.SEVERE, sql, e);
            }
            finally
            {
                dt = null;
            }
            return retValue;
        }

        /// <summary>
        /// Get BPartner with Value
        /// </summary>
        /// <param name="ctx">context</param>
        /// <param name="C_BPartner_ID">id</param>
        /// <returns>BPartner or null</returns>
        public static MBPartner Get(Ctx ctx, int C_BPartner_ID)
        {
            MBPartner retValue = null;
            int AD_Client_ID = ctx.GetAD_Client_ID();
            String sql = "SELECT * FROM C_BPartner WHERE C_BPartner_ID=" + C_BPartner_ID
                + " AND  AD_Client_ID=" + AD_Client_ID;
            DataSet ds = new DataSet();
            try
            {
                ds = DataBase.DB.ExecuteDataset(sql, null, null);
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    DataRow dr = ds.Tables[0].Rows[i];
                    retValue = new MBPartner(ctx, dr, null);
                }
                ds = null;
            }
            catch (Exception e)
            {
                _log.Log(Level.SEVERE, sql, e);
            }
            return retValue;
        }

        /// <summary>
        /// Get Not Invoiced Shipment Value
        /// </summary>
        /// <param name="C_BPartner_ID">partner</param>
        /// <returns>value in accounting currency</returns>
        public static Decimal GetNotInvoicedAmt(int C_BPartner_ID)
        {
            Decimal retValue = new decimal();
            String sql = "SELECT SUM(COALESCE("
                + "currencyBase((ol.QtyDelivered-ol.QtyInvoiced)*ol.PriceActual,o.C_Currency_ID,o.DateOrdered, o.AD_Client_ID,o.AD_Org_ID) ,0)) "
                + " FROM C_OrderLine ol"
                + " INNER JOIN C_Order o ON (ol.C_Order_ID=o.C_Order_ID) "
                + " WHERE o.IsSOTrx='Y' AND Bill_BPartner_ID=" + C_BPartner_ID;
            IDataReader idr = null;
            try
            {
                idr = DataBase.DB.ExecuteReader(sql, null, null);
                if (idr.Read())
                {
                    retValue = Utility.Util.GetValueOfDecimal(idr[0]);
                }
                idr.Close();
            }
            catch (Exception e)
            {
                if (idr != null)
                {
                    idr.Close();
                }
                _log.Log(Level.SEVERE, sql, e);
            }
            return retValue;
        }

        /// <summary>
        /// Constructor for new BPartner from Template
        /// </summary>
        /// <param name="ctx">context</param>
        /// <param name="trxName">transaction</param>
        public MBPartner(Ctx ctx, Trx trxName)
            : this(ctx, -1, trxName)
        {

        }

        /// <summary>
        /// Default Constructor
        /// </summary>
        /// <param name="ctx">context</param>
        /// <param name="dr">data row</param>
        /// <param name="trxName">transaction</param>
        public MBPartner(Ctx ctx, DataRow dr, Trx trxName)
            : base(ctx, dr, trxName)
        {
        }

        /// <summary>
        /// Default Constructor
        /// </summary>
        /// <param name="ctx">context</param>
        /// <param name="C_BPartner_ID">id</param>
        /// <param name="trxName">transaction</param>
        public MBPartner(Ctx ctx, int C_BPartner_ID, Trx trxName)
            : base(ctx, C_BPartner_ID, trxName)
        {
            //
            if (C_BPartner_ID == -1)
            {
                InitTemplate(ctx.GetContextAsInt("AD_Client_ID"));
                C_BPartner_ID = 0;
            }
            if (C_BPartner_ID == 0)
            {
                //	setValue ("");
                //	setName ("");
                //	setName2 (null);
                //	setDUNS("");
                //
                SetIsCustomer(true);
                SetIsProspect(true);
                //
                SetSendEMail(false);
                SetIsOneTime(false);
                SetIsVendor(false);
                SetIsSummary(false);
                SetIsEmployee(false);
                SetIsSalesRep(false);
                SetIsTaxExempt(false);
                SetIsDiscountPrinted(false);
                //
                SetSO_CreditLimit(Env.ZERO);
                SetSO_CreditUsed(Env.ZERO);
                SetTotalOpenBalance(Env.ZERO);
                SetSOCreditStatus(SOCREDITSTATUS_NoCreditCheck);
                //
                SetFirstSale(null);
                SetActualLifeTimeValue(Env.ZERO);
                SetPotentialLifeTimeValue(Env.ZERO);
                SetAcqusitionCost(Env.ZERO);
                SetShareOfCustomer(0);
                SetSalesVolume(0);
            }
            log.Fine(ToString());
        }

        /// <summary>
        /// Import Contstructor
        /// </summary>
        /// <param name="impBP">import</param>
        public MBPartner(X_I_BPartner impBP)
            : this(impBP.GetCtx(), 0, impBP.Get_TrxName())
        {

            SetClientOrg(impBP);
            SetUpdatedBy(impBP.GetUpdatedBy());
            //
            String value = impBP.GetValue();
            if (value == null || value.Length == 0)
                value = impBP.GetEMail();
            if (value == null || value.Length == 0)
                value = impBP.GetContactName();
            SetValue(value);
            String name = impBP.GetName();
            if (name == null || name.Length == 0)
                name = impBP.GetContactName();
            if (name == null || name.Length == 0)
                name = impBP.GetEMail();
            SetName(name);
            SetName2(impBP.GetName2());
            SetDescription(impBP.GetDescription());
            //	setHelp(impBP.getHelp());
            SetDUNS(impBP.GetDUNS());
            SetTaxID(impBP.GetTaxID());
            SetNAICS(impBP.GetNAICS());
            SetC_BP_Group_ID(impBP.GetC_BP_Group_ID());
        }

        /// <summary>
        /// Load Default BPartner
        /// </summary>
        /// <param name="AD_Client_ID">client id</param>
        /// <returns>true if loaded</returns>
        private bool InitTemplate(int AD_Client_ID)
        {
            if (AD_Client_ID == 0)
                throw new ArgumentException("Client_ID=0");

            bool success = true;
            String sql = "SELECT * FROM C_BPartner "
                + " WHERE C_BPartner_ID=(SELECT C_BPartnerCashTrx_ID FROM AD_ClientInfo" +
                " WHERE AD_Client_ID=" + AD_Client_ID + ")";
            try
            {
                DataSet ds = DataBase.DB.ExecuteDataset(sql, null, null);
                if (ds.Tables.Count != 0 && ds.Tables[0].Rows.Count > 0)
                {
                    success = Load(ds.Tables[0].Rows[0]);
                }
                else
                {
                    Load(0, (Trx)null);
                    success = false;
                    log.Severe("None found");
                }
            }
            catch (Exception e)
            {
                log.Log(Level.SEVERE, sql, e);
            }

            SetStandardDefaults();
            //	Reset
            Set_ValueNoCheck("C_BPartner_ID", I_ZERO);
            SetValue("");
            SetName("");
            SetName2(null);
            return success;
        }

        /// <summary>
        /// Get All Contacts
        /// </summary>
        /// <param name="reload">if true users will be requeried</param>
        /// <returns>contacts</returns>
        public MUser[] GetContacts(bool reload)
        {
            if (reload || _contacts == null || _contacts.Length == 0)
            {
                ;
            }
            else
                return _contacts;
            //
            List<MUser> list = new List<MUser>();
            String sql = "SELECT * FROM AD_User WHERE C_BPartner_ID=" + GetC_BPartner_ID();
            DataSet ds = null;
            try
            {
                ds = DataBase.DB.ExecuteDataset(sql, null, null);
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    DataRow dr = ds.Tables[0].Rows[i];
                    list.Add(new MUser(GetCtx(), dr, null));
                }
                ds = null;
            }
            catch (Exception e)
            {
                log.Log(Level.SEVERE, sql, e);
            }

            _contacts = new MUser[list.Count];
            _contacts = list.ToArray();
            return _contacts;
        }

        /// <summary>
        /// Get specified or first Contact
        /// </summary>
        /// <param name="AD_User_ID">optional user</param>
        /// <returns>contact or null</returns>
        public MUser GetContact(int AD_User_ID)
        {
            MUser[] users = GetContacts(false);
            if (users.Length == 0)
                return null;
            for (int i = 0; AD_User_ID != 0 && i < users.Length; i++)
            {
                if (users[i].GetAD_User_ID() == AD_User_ID)
                    return users[i];
            }
            return users[0];
        }

        /// <summary>
        /// Get All Locations
        /// </summary>
        /// <param name="reload">if true locations will be requeried</param>
        /// <returns>locations</returns>
        public MBPartnerLocation[] GetLocations(bool reload)
        {
            if (reload || _locations == null || _locations.Length == 0)
            {
                ;
            }
            else
            {
                return _locations;
            }
            //
            List<MBPartnerLocation> list = new List<MBPartnerLocation>();
            String sql = "SELECT * FROM C_BPartner_Location WHERE C_BPartner_ID=" + GetC_BPartner_ID();
            DataSet ds = null;
            try
            {
                ds = DataBase.DB.ExecuteDataset(sql, null, Get_TrxName());
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    DataRow dr = ds.Tables[0].Rows[i];
                    list.Add(new MBPartnerLocation(GetCtx(), dr, Get_TrxName()));
                }
                ds = null;
            }
            catch (Exception e)
            {
                log.Log(Level.SEVERE, sql, e);
            }

            _locations = new MBPartnerLocation[list.Count];
            _locations = list.ToArray();
            return _locations;
        }

        /// <summary>
        /// Get explicit or first bill Location
        /// </summary>
        /// <param name="C_BPartner_Location_ID">optional explicit location</param>
        /// <returns>location or null</returns>
        public MBPartnerLocation GetLocation(int C_BPartner_Location_ID)
        {
            MBPartnerLocation[] locations = GetLocations(false);
            if (locations.Length == 0)
                return null;
            MBPartnerLocation retValue = null;
            for (int i = 0; i < locations.Length; i++)
            {
                if (locations[i].GetC_BPartner_Location_ID() == C_BPartner_Location_ID)
                    return locations[i];
                if (retValue == null && locations[i].IsBillTo())
                    retValue = locations[i];
            }
            if (retValue == null)
                return locations[0];
            return retValue;
        }

        /// <summary>
        /// Get Bank Accounts
        /// </summary>
        /// <param name="requery">requery</param>
        /// <returns>Bank Accounts</returns>
        public MBPBankAccount[] GetBankAccounts(bool requery)
        {
            if (_accounts != null && _accounts.Length >= 0 && !requery)	//	re-load
                return _accounts;
            //
            List<MBPBankAccount> list = new List<MBPBankAccount>();
            String sql = "SELECT * FROM C_BP_BankAccount WHERE C_BPartner_ID=" + GetC_BPartner_ID()
                + " AND IsActive='Y'";
            DataTable dt = null;
            IDataReader idr = null;
            try
            {
                idr = DataBase.DB.ExecuteReader(sql, null, Get_TrxName());
                dt = new DataTable();
                dt.Load(idr);
                idr.Close();
                foreach (DataRow dr in dt.Rows)
                {
                    list.Add(new MBPBankAccount(GetCtx(), dr, Get_TrxName()));
                }
            }
            catch (Exception e)
            {
                if (idr != null)
                {
                    idr.Close();
                }
                log.Log(Level.SEVERE, sql, e);
            }
            finally { dt = null; }

            _accounts = new MBPBankAccount[list.Count];
            _accounts = list.ToArray();
            return _accounts;
        }

        /// <summary>
        /// String Representation
        /// </summary>
        /// <returns>info</returns>
        public override String ToString()
        {
            StringBuilder sb = new StringBuilder("MBPartner[ID=")
                .Append(Get_ID())
                .Append(",Value=").Append(GetValue())
                .Append(",Name=").Append(GetName())
                .Append(",OpenBalance=").Append(GetTotalOpenBalance())
                .Append("]");
            return sb.ToString();
        }

        /// <summary>
        /// Set Client/Org
        /// </summary>
        /// <param name="AD_Client_ID">client</param>
        /// <param name="AD_Org_ID">org</param>
        //public void SetClientOrg(int AD_Client_ID, int AD_Org_ID)
        //{
        //    base.SetClientOrg(AD_Client_ID, AD_Org_ID);
        //}

        /// <summary>
        /// Set Linked Organization.
        /// </summary>
        /// <param name="AD_OrgBP_ID">id</param>
        public void SetAD_OrgBP_ID(int AD_OrgBP_ID)
        {
            if (AD_OrgBP_ID == 0)
                base.SetAD_OrgBP_ID(null);
            else
                base.SetAD_OrgBP_ID(AD_OrgBP_ID.ToString());
        }

        /// <summary>
        /// Get Linked Organization.
        ///	(is Button)
        /// The Business Partner is another Organization 
        ///	for explicit Inter-Org transactions 
        /// </summary>
        /// <returns>AD_OrgBP_ID if BP</returns>
        public int GetAD_OrgBP_ID_Int()
        {
            String org = base.GetAD_OrgBP_ID();
            if (org == null)
                return 0;
            int AD_OrgBP_ID = 0;
            try
            {
                AD_OrgBP_ID = int.Parse(org);
            }
            catch (Exception ex)
            {
                log.Log(Level.SEVERE, org, ex);
            }
            return AD_OrgBP_ID;
        }

        /// <summary>
        /// Get Primary C_BPartner_Location_ID
        /// </summary>
        /// <returns>C_BPartner_Location_ID</returns>
        public int GetPrimaryC_BPartner_Location_ID()
        {
            if (_primaryC_BPartner_Location_ID == null)
            {
                MBPartnerLocation[] locs = GetLocations(false);
                for (int i = 0; _primaryC_BPartner_Location_ID == null && i < locs.Length; i++)
                {
                    if (locs[i].IsBillTo())
                    {
                        SetPrimaryC_BPartner_Location_ID(locs[i].GetC_BPartner_Location_ID());
                        break;
                    }
                }
                //	get first
                if (_primaryC_BPartner_Location_ID == null && locs.Length > 0)
                    SetPrimaryC_BPartner_Location_ID(locs[0].GetC_BPartner_Location_ID());
            }
            if (_primaryC_BPartner_Location_ID == null)
                return 0;
            return (int)_primaryC_BPartner_Location_ID;
        }

        /// <summary>
        /// Get Primary C_BPartner_Location
        /// </summary>
        /// <returns>C_BPartner_Location</returns>
        public MBPartnerLocation GetPrimaryC_BPartner_Location()
        {
            if (_primaryC_BPartner_Location_ID == null)
            {
                _primaryC_BPartner_Location_ID = GetPrimaryC_BPartner_Location_ID();
            }
            if (_primaryC_BPartner_Location_ID == null)
                return null;
            return new MBPartnerLocation(GetCtx(), (int)_primaryC_BPartner_Location_ID, null);
        }

        /// <summary>
        /// Get Primary AD_User_ID
        /// </summary>
        /// <returns>AD_User_ID</returns>
        public int GetPrimaryAD_User_ID()
        {
            if (_primaryAD_User_ID == null)
            {
                MUser[] users = GetContacts(false);
                //	for (int i = 0; i < users.Length; i++)
                //	{
                //	}
                if (_primaryAD_User_ID == null && users.Length > 0)
                    SetPrimaryAD_User_ID(users[0].GetAD_User_ID());
            }
            if (_primaryAD_User_ID == null)
                return 0;
            return (int)_primaryAD_User_ID;
        }

        /// <summary>
        /// Set Primary C_BPartner_Location_ID
        /// </summary>
        /// <param name="C_BPartner_Location_ID">id</param>
        public void SetPrimaryC_BPartner_Location_ID(int C_BPartner_Location_ID)
        {
            _primaryC_BPartner_Location_ID = C_BPartner_Location_ID;
        }

        /// <summary>
        /// Set Primary AD_User_ID
        /// </summary>
        /// <param name="AD_User_ID">id</param>
        public void SetPrimaryAD_User_ID(int AD_User_ID)
        {
            _primaryAD_User_ID = AD_User_ID;
        }

        /// <summary>
        /// Calculate Total Open Balance and SO_CreditUsed.
        /// (includes drafted invoices)
        /// </summary>
        public void SetTotalOpenBalance()
        {
            Decimal? SO_CreditUsed = null;
            Decimal? TotalOpenBalance = null;
            String sql = "SELECT "
                //	SO Credit Used	= SO Invoices
                + "COALESCE((SELECT SUM(currencyBase(i.GrandTotal,i.C_Currency_ID,i.DateOrdered, i.AD_Client_ID,i.AD_Org_ID)) "
                    + " FROM C_Invoice_v i "
                    + " WHERE i.C_BPartner_ID=bp.C_BPartner_ID"
                    + " AND i.IsSOTrx='Y' AND i.DocStatus IN('CO','CL')),0) "
                //					- All SO Allocations
                + "-COALESCE((SELECT SUM(currencyBase(a.Amount+a.DiscountAmt+a.WriteoffAmt,i.C_Currency_ID,i.DateOrdered,a.AD_Client_ID,a.AD_Org_ID)) "
                    + " FROM C_AllocationLine a INNER JOIN C_Invoice i ON (a.C_Invoice_ID=i.C_Invoice_ID) "
                    + " INNER JOIN C_AllocationHdr h ON (a.C_AllocationHdr_ID = h.C_AllocationHdr_ID) "
                    + " WHERE a.C_BPartner_ID=bp.C_BPartner_ID AND a.IsActive='Y'"
                    + " AND i.isSoTrx='Y' AND h.DocStatus IN('CO','CL')),0) "
                //					- Unallocated Receipts	= (All Receipts
                + "-(SELECT COALESCE(SUM(currencyBase(p.PayAmt+p.DiscountAmt+p.WriteoffAmt,p.C_Currency_ID,p.DateTrx,p.AD_Client_ID,p.AD_Org_ID)),0) "
                    + " FROM C_Payment_v p "
                    + " WHERE p.C_BPartner_ID=bp.C_BPartner_ID"
                    + " AND p.IsReceipt='Y' AND p.DocStatus IN('CO','CL')"
                    + " AND p.C_Charge_ID IS NULL)"
                //											- All Receipt Allocations
                + "+(SELECT COALESCE(SUM(currencyBase(a.Amount+a.DiscountAmt+a.WriteoffAmt,i.C_Currency_ID,i.DateOrdered,a.AD_Client_ID,a.AD_Org_ID)),0) "
                    + " FROM C_AllocationLine a INNER JOIN C_Invoice i ON (a.C_Invoice_ID=i.C_Invoice_ID) "
                    + " INNER JOIN C_AllocationHdr h ON (a.C_AllocationHdr_ID = h.C_AllocationHdr_ID) "
                    + " WHERE a.C_BPartner_ID=bp.C_BPartner_ID"
                    + " AND a.IsActive='Y' AND a.C_Payment_ID IS NOT NULL"
                    + " AND i.isSoTrx='Y' AND h.DocStatus IN('CO','CL')), "

                //	Balance			= All Invoices
                + "COALESCE((SELECT SUM(currencyBase(i.GrandTotal*MultiplierAP,i.C_Currency_ID,i.DateOrdered, i.AD_Client_ID,i.AD_Org_ID)) "
                    + " FROM C_Invoice_v i "
                    + " WHERE i.C_BPartner_ID=bp.C_BPartner_ID"
                    + " AND i.DocStatus IN('CO','CL')),0) "
                //					- All Allocations
                + "-COALESCE((SELECT SUM(currencyBase(a.Amount+a.DiscountAmt+a.WriteoffAmt,i.C_Currency_ID,i.DateOrdered,a.AD_Client_ID,a.AD_Org_ID)) "
                    + " FROM C_AllocationLine a INNER JOIN C_Invoice i ON (a.C_Invoice_ID=i.C_Invoice_ID) "
                    + " INNER JOIN C_AllocationHdr h ON (a.C_AllocationHdr_ID = h.C_AllocationHdr_ID) "
                    + " WHERE a.C_BPartner_ID=bp.C_BPartner_ID AND a.IsActive='Y' AND h.DocStatus IN('CO','CL')),0) "
                //					- Unallocated Receipts	= (All Receipts
                + "-(SELECT COALESCE(SUM(currencyBase(p.PayAmt+p.DiscountAmt+p.WriteoffAmt,p.C_Currency_ID,p.DateTrx,p.AD_Client_ID,p.AD_Org_ID)),0) "
                    + " FROM C_Payment_v p "
                    + " WHERE p.C_BPartner_ID=bp.C_BPartner_ID"
                    + " AND p.DocStatus IN('CO','CL')"
                    + " AND p.C_Charge_ID IS NULL)"
                //											- All Allocations
                + "+(SELECT COALESCE(SUM(currencyBase(a.Amount+a.DiscountAmt+a.WriteoffAmt,i.C_Currency_ID,i.DateOrdered,a.AD_Client_ID,a.AD_Org_ID)),0) "
                    + " FROM C_AllocationLine a INNER JOIN C_Invoice i ON (a.C_Invoice_ID=i.C_Invoice_ID) "
                    + " INNER JOIN C_AllocationHdr h ON (a.C_AllocationHdr_ID = h.C_AllocationHdr_ID) "
                    + " WHERE a.C_BPartner_ID=bp.C_BPartner_ID"
                    + " AND a.IsActive='Y' AND a.C_Payment_ID IS NOT NULL AND h.DocStatus IN('CO','CL')) "
                //
                + " FROM C_BPartner bp "
                + " WHERE C_BPartner_ID=" + GetC_BPartner_ID();
            DataTable dt = null;
            IDataReader idr = null;
            try
            {
                idr = DataBase.DB.ExecuteReader(sql, null, Get_TrxName());
                dt = new DataTable();
                dt.Load(idr);
                idr.Close();
                foreach (DataRow dr in dt.Rows)
                {
                    SO_CreditUsed = Utility.Util.GetValueOfDecimal(dr[0]);
                    TotalOpenBalance = Utility.Util.GetValueOfDecimal(dr[1]);
                }
            }
            catch (Exception e)
            {
                if (idr != null)
                {
                    idr.Close();
                }
                log.Log(Level.SEVERE, sql, e);
            }
            finally { dt = null; }

            _TotalOpenBalanceSet = true;
            String info = null;
            if (SO_CreditUsed != null)
            {
                info = "SO_CreditUsed=" + SO_CreditUsed;
                base.SetSO_CreditUsed(Convert.ToDecimal(SO_CreditUsed));
            }

            if (TotalOpenBalance != null)
            {
                if (info != null)
                    info += ", ";
                info += "TotalOpenBalance=" + TotalOpenBalance;
                base.SetTotalOpenBalance(Convert.ToDecimal(TotalOpenBalance));
            }
            log.Fine(info);
            SetSOCreditStatus();
        }

        /// <summary>
        /// Set Actual Life Time Value from DB
        /// </summary>
        public void SetActualLifeTimeValue()
        {
            Decimal? ActualLifeTimeValue = null;
            String sql = "SELECT "
                + "COALESCE ((SELECT SUM(currencyBase(i.GrandTotal,i.C_Currency_ID,i.DateOrdered,"
                + " i.AD_Client_ID,i.AD_Org_ID)) "
                    + " FROM C_Invoice_v i WHERE i.C_BPartner_ID=bp.C_BPartner_ID AND i.IsSOTrx='Y'"
            + " AND i.DocStatus IN('CO','CL')),0) FROM C_BPartner bp "
                + " WHERE C_BPartner_ID=" + GetC_BPartner_ID();
            DataTable dt = null;
            IDataReader idr = null;
            try
            {
                idr = DataBase.DB.ExecuteReader(sql, null, Get_TrxName());
                dt = new DataTable();
                dt.Load(idr);
                idr.Close();
                foreach (DataRow dr in dt.Rows)
                {
                    ActualLifeTimeValue = Utility.Util.GetValueOfDecimal(dr[0]);
                }
            }
            catch (Exception e)
            {
                if (idr != null)
                {
                    idr.Close();
                }
                log.Log(Level.SEVERE, sql, e);
            }
            finally { dt = null; }

            if (ActualLifeTimeValue != null)
                base.SetActualLifeTimeValue(Convert.ToDecimal(ActualLifeTimeValue));
        }

        /// <summary>
        /// Get Total Open Balance
        /// </summary>
        /// <param name="calculate">if null calculate it</param>
        /// <returns>open balance</returns>
        public Decimal GetTotalOpenBalance(bool calculate)
        {
            if (Env.Signum(GetTotalOpenBalance()) == 0 && calculate)
                SetTotalOpenBalance();
            return base.GetTotalOpenBalance();
        }

        /// <summary>
        /// Set Credit Status
        /// </summary>
        public void SetSOCreditStatus()
        {
            Decimal creditLimit = GetSO_CreditLimit();
            //	Nothing to do
            if (SOCREDITSTATUS_NoCreditCheck.Equals(GetSOCreditStatus())
                || SOCREDITSTATUS_CreditStop.Equals(GetSOCreditStatus())
                || Env.ZERO.CompareTo(creditLimit) == 0)
                return;

            //	Above Credit Limit
            if (creditLimit.CompareTo(GetTotalOpenBalance(!_TotalOpenBalanceSet)) < 0)
                SetSOCreditStatus(SOCREDITSTATUS_CreditHold);
            else
            {
                //	Above Watch Limit
                Decimal watchAmt = Decimal.Multiply(creditLimit, GetCreditWatchRatio());
                if (watchAmt.CompareTo(GetTotalOpenBalance()) < 0)
                    SetSOCreditStatus(SOCREDITSTATUS_CreditWatch);
                else	//	is OK
                    SetSOCreditStatus(SOCREDITSTATUS_CreditOK);
            }
            log.Fine("SOCreditStatus=" + GetSOCreditStatus());
        }

        /// <summary>
        /// Get SO CreditStatus with additional amount
        /// </summary>
        /// <param name="additionalAmt">additional amount in Accounting Currency</param>
        /// <returns>sinulated credit status</returns>
        public String GetSOCreditStatus(Decimal? additionalAmt)
        {
            if (additionalAmt == null || Env.Signum((Decimal)additionalAmt) == 0)
                return GetSOCreditStatus();
            //
            Decimal creditLimit = GetSO_CreditLimit();
            //	Nothing to do
            if (SOCREDITSTATUS_NoCreditCheck.Equals(GetSOCreditStatus())
                || SOCREDITSTATUS_CreditStop.Equals(GetSOCreditStatus())
                || Env.ZERO.CompareTo(creditLimit) == 0)
                return GetSOCreditStatus();
            //	Above (reduced) Credit Limit
            creditLimit = Decimal.Subtract(creditLimit, (Decimal)additionalAmt);
            if (creditLimit.CompareTo(GetTotalOpenBalance(!_TotalOpenBalanceSet)) < 0)
                return SOCREDITSTATUS_CreditHold;

            //	Above Watch Limit
            Decimal watchAmt = Decimal.Multiply(creditLimit, GetCreditWatchRatio());
            if (watchAmt.CompareTo(GetTotalOpenBalance()) < 0)
                return SOCREDITSTATUS_CreditWatch;
            //	is OK
            return SOCREDITSTATUS_CreditOK;
        }

        /// <summary>
        /// Get Credit Watch Ratio
        /// </summary>
        /// <returns>BP Group ratio or 0.9</returns>
        public Decimal GetCreditWatchRatio()
        {
            return GetBPGroup().GetCreditWatchRatio();
        }

        /// <summary>
        /// Credit Status is Stop or Hold.
        /// </summary>
        /// <returns>true if Stop/Hold</returns>
        public bool IsCreditStopHold()
        {
            String status = GetSOCreditStatus();
            return SOCREDITSTATUS_CreditStop.Equals(status)
                || SOCREDITSTATUS_CreditHold.Equals(status);
        }

        /// <summary>
        /// Set Total Open Balance
        /// </summary>
        /// <param name="TotalOpenBalance">Total Open Balance</param>
        public void SetTotalOpenBalance(Decimal TotalOpenBalance)
        {
            _TotalOpenBalanceSet = false;
            base.SetTotalOpenBalance(TotalOpenBalance);
        }

        /// <summary>
        /// Get BP Group
        /// </summary>
        /// <returns>group</returns>
        public MBPGroup GetBPGroup()
        {
            if (_group == null)
            {
                if (GetC_BP_Group_ID() == 0)
                    _group = MBPGroup.GetDefault(GetCtx());
                else
                    _group = MBPGroup.Get(GetCtx(), GetC_BP_Group_ID());
            }
            return _group;
        }

        /// <summary>
        /// Get BP Group
        /// </summary>
        /// <param name="group">group</param>
        public void SetBPGroup(MBPGroup group)
        {
            _group = group;
            if (_group == null)
                return;
            SetC_BP_Group_ID(_group.GetC_BP_Group_ID());
            if (_group.GetC_Dunning_ID() != 0)
                SetC_Dunning_ID(_group.GetC_Dunning_ID());
            if (_group.GetM_PriceList_ID() != 0)
                SetM_PriceList_ID(_group.GetM_PriceList_ID());
            if (_group.GetPO_PriceList_ID() != 0)
                SetPO_PriceList_ID(_group.GetPO_PriceList_ID());
            if (_group.GetM_DiscountSchema_ID() != 0)
                SetM_DiscountSchema_ID(_group.GetM_DiscountSchema_ID());
            if (_group.GetPO_DiscountSchema_ID() != 0)
                SetPO_DiscountSchema_ID(_group.GetPO_DiscountSchema_ID());
        }

        /// <summary>
        /// Get PriceList
        /// </summary>
        /// <returns>price list</returns>
        public new int GetM_PriceList_ID()
        {
            int ii = base.GetM_PriceList_ID();
            if (ii == 0)
                ii = GetBPGroup().GetM_PriceList_ID();
            return ii;
        }

        /// <summary>
        /// Get PO PriceList
        /// </summary>
        /// <returns>price list</returns>
        public new int GetPO_PriceList_ID()
        {
            int ii = base.GetPO_PriceList_ID();
            if (ii == 0)
                ii = GetBPGroup().GetPO_PriceList_ID();
            return ii;
        }

        /// <summary>
        /// Get DiscountSchema
        /// </summary>
        /// <returns>Discount Schema</returns>
        public new int GetM_DiscountSchema_ID()
        {
            int ii = base.GetM_DiscountSchema_ID();
            if (ii == 0)
                ii = GetBPGroup().GetM_DiscountSchema_ID();
            return ii;
        }

        /// <summary>
        /// Get PO DiscountSchema
        /// </summary>
        /// <returns>PO Discount</returns>
        public new int GetPO_DiscountSchema_ID()
        {
            int ii = base.GetPO_DiscountSchema_ID();
            if (ii == 0)
                ii = GetBPGroup().GetPO_DiscountSchema_ID();
            return ii;
        }

        /// <summary>
        /// Get ReturnPolicy
        /// </summary>
        /// <returns>Return Policy</returns>
        public new int GetM_ReturnPolicy_ID()
        {
            int ii = base.GetM_ReturnPolicy_ID();
            if (ii == 0)
                ii = GetBPGroup().GetM_ReturnPolicy_ID();
            if (ii == 0)
                ii = MReturnPolicy.GetDefault(GetCtx());
            return ii;
        }

        /// <summary>
        /// Get Vendor ReturnPolicy
        /// </summary>
        /// <returns>Return Policy</returns>
        public new int GetPO_ReturnPolicy_ID()
        {
            int ii = base.GetPO_ReturnPolicy_ID();
            if (ii == 0)
                ii = GetBPGroup().GetPO_ReturnPolicy_ID();
            if (ii == 0)
                ii = MReturnPolicy.GetDefault(GetCtx());
            return ii;
        }

        /// <summary>
        /// Before Save
        /// </summary>
        /// <param name="newRecord">new</param>
        /// <returns>true</returns>
        protected override bool BeforeSave(bool newRecord)
        {
            if (newRecord || Is_ValueChanged("C_BP_Group_ID"))
            {
                MBPGroup grp = GetBPGroup();
                if (grp == null)
                {
                    log.SaveWarning("Error", Msg.ParseTranslation(GetCtx(), "@NotFound@:  @C_BP_Group_ID@"));
                    return false;
                }
                SetBPGroup(grp);	//	setDefaults
            }
            return true;
        }

        /// <summary>
        /// After save
        /// </summary>
        /// <param name="newRecord">new</param>
        /// <param name="success">success</param>
        /// <returns>success</returns>
        protected override bool AfterSave(bool newRecord, bool success)
        {
            if (newRecord & success)
            {
                //	Accounting
                Insert_Accounting("C_BP_Customer_Acct", "C_BP_Group_Acct",
                    "p.C_BP_Group_ID=" + GetC_BP_Group_ID());
                Insert_Accounting("C_BP_Vendor_Acct", "C_BP_Group_Acct",
                    "p.C_BP_Group_ID=" + GetC_BP_Group_ID());
                Insert_Accounting("C_BP_Employee_Acct", "C_AcctSchema_Default", null);
            }

            //	Value/Name change
            if (success && !newRecord
                && (Is_ValueChanged("Value") || Is_ValueChanged("Name")))
                MAccount.UpdateValueDescription(GetCtx(), "C_BPartner_ID=" +
                    GetC_BPartner_ID(), Get_TrxName());

            return success;
        }

        /// <summary>
        /// Before Delete
        /// </summary>
        /// <returns>true</returns>
        protected override bool BeforeDelete()
        {
            return Delete_Accounting("C_BP_Customer_Acct")
                && Delete_Accounting("C_BP_Vendor_Acct")
                && Delete_Accounting("C_BP_Employee_Acct");
        }
    }
}