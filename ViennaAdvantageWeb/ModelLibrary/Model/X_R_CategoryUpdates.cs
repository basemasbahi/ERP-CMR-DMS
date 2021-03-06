namespace VAdvantage.Model
{

/** Generated Model - DO NOT CHANGE */
using System;
using System.Text;
using VAdvantage.DataBase;
using VAdvantage.Common;
using VAdvantage.Classes;
using VAdvantage.Process;
using VAdvantage.Model;
using VAdvantage.Utility;
using System.Data;
/** Generated Model for R_CategoryUpdates
 *  @author Jagmohan Bhatt (generated) 
 *  @version Vienna Framework 1.1.1 - $Id$ */
public class X_R_CategoryUpdates : PO
{
public X_R_CategoryUpdates (Context ctx, int R_CategoryUpdates_ID, Trx trxName) : base (ctx, R_CategoryUpdates_ID, trxName)
{
/** if (R_CategoryUpdates_ID == 0)
{
SetAD_User_ID (0);
SetIsSelfService (false);
SetR_Category_ID (0);
}
 */
}
public X_R_CategoryUpdates (Ctx ctx, int R_CategoryUpdates_ID, Trx trxName) : base (ctx, R_CategoryUpdates_ID, trxName)
{
/** if (R_CategoryUpdates_ID == 0)
{
SetAD_User_ID (0);
SetIsSelfService (false);
SetR_Category_ID (0);
}
 */
}
/** Load Constructor 
@param ctx context
@param rs result set 
@param trxName transaction
*/
public X_R_CategoryUpdates (Context ctx, DataRow rs, Trx trxName) : base(ctx, rs, trxName)
{
}
/** Load Constructor 
@param ctx context
@param rs result set 
@param trxName transaction
*/
public X_R_CategoryUpdates (Ctx ctx, DataRow rs, Trx trxName) : base(ctx, rs, trxName)
{
}
/** Load Constructor 
@param ctx context
@param rs result set 
@param trxName transaction
*/
public X_R_CategoryUpdates (Ctx ctx, IDataReader dr, Trx trxName) : base(ctx, dr, trxName)
{
}
/** Static Constructor 
 Set Table ID By Table Name
 added by ->Harwinder */
static X_R_CategoryUpdates()
{
 Table_ID = Get_Table_ID(Table_Name);
 model = new KeyNamePair(Table_ID,Table_Name);
}
/** Serial Version No */
//static long serialVersionUID 27562514382704L;
/** Last Updated Timestamp 7/29/2010 1:07:45 PM */
public static long updatedMS = 1280389065915L;
/** AD_Table_ID=785 */
public static int Table_ID;
 // =785;

/** TableName=R_CategoryUpdates */
public static String Table_Name="R_CategoryUpdates";

protected static KeyNamePair model;
protected Decimal accessLevel = new Decimal(7);
/** AccessLevel
@return 7 - System - Client - Org 
*/
protected override int Get_AccessLevel()
{
return Convert.ToInt32(accessLevel.ToString());
}
/** Load Meta Data
@param ctx context
@return PO Info
*/
protected override POInfo InitPO (Ctx ctx)
{
POInfo poi = POInfo.GetPOInfo (ctx, Table_ID);
return poi;
}
/** Load Meta Data
@param ctx context
@return PO Info
*/
protected override POInfo InitPO(Context ctx)
{
POInfo poi = POInfo.GetPOInfo (ctx, Table_ID);
return poi;
}
/** Info
@return info
*/
public override String ToString()
{
StringBuilder sb = new StringBuilder ("X_R_CategoryUpdates[").Append(Get_ID()).Append("]");
return sb.ToString();
}
/** Set User/Contact.
@param AD_User_ID User within the system - Internal or Business Partner Contact */
public void SetAD_User_ID (int AD_User_ID)
{
if (AD_User_ID < 1) throw new ArgumentException ("AD_User_ID is mandatory.");
Set_ValueNoCheck ("AD_User_ID", AD_User_ID);
}
/** Get User/Contact.
@return User within the system - Internal or Business Partner Contact */
public int GetAD_User_ID() 
{
Object ii = Get_Value("AD_User_ID");
if (ii == null) return 0;
return Convert.ToInt32(ii);
}
/** Set Self-Service.
@param IsSelfService This is a Self-Service entry or this entry can be changed via Self-Service */
public void SetIsSelfService (Boolean IsSelfService)
{
Set_Value ("IsSelfService", IsSelfService);
}
/** Get Self-Service.
@return This is a Self-Service entry or this entry can be changed via Self-Service */
public Boolean IsSelfService() 
{
Object oo = Get_Value("IsSelfService");
if (oo != null) 
{
 if (oo.GetType() == typeof(bool)) return Convert.ToBoolean(oo);
 return "Y".Equals(oo);
}
return false;
}
/** Set Category.
@param R_Category_ID Request Category */
public void SetR_Category_ID (int R_Category_ID)
{
if (R_Category_ID < 1) throw new ArgumentException ("R_Category_ID is mandatory.");
Set_ValueNoCheck ("R_Category_ID", R_Category_ID);
}
/** Get Category.
@return Request Category */
public int GetR_Category_ID() 
{
Object ii = Get_Value("R_Category_ID");
if (ii == null) return 0;
return Convert.ToInt32(ii);
}
/** Get Record ID/ColumnName
@return ID/ColumnName pair */
public KeyNamePair GetKeyNamePair() 
{
return new KeyNamePair(Get_ID(), GetR_Category_ID().ToString());
}
}

}
