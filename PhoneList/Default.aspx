<%@ Page Language="VB" AutoEventWireup="false" CodeFile="Default.aspx.vb" Inherits="_Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>
    </title>
    <style type="text/css">
        body,form{background:url(images/wallpaper0.jpg);font-size:8pt;font-family:Arial;width:100%;margin:0}
        .div1{width:1020px;margin:30px auto}
        table{width:1020px;background:#dffaf4;font-family:Arial;border-collapse:collapse;text-align:left}
        th{background:#c3dbd6;font-weight:bold;font-size:8pt;text-align:left;border:1px solid #ffffff;padding-left:4px;padding-right:4px;COLOR: #e48625}
        td{font-size:8pt;border-left:1px solid #efefef;padding-left:4px;padding-right:4px}
        
        a:link {FONT-WEIGHT: bold; FONT-SIZE: 11px; COLOR: #e48625; FONT-FAMILY: Arial, Helvetica, sans-serif; TEXT-DECORATION: none}
        
        a:visited {FONT-WEIGHT: bold;FONT-SIZE: 11px; COLOR: #e48625; FONT-FAMILY: Arial, Helvetica, sans-serif; TEXT-DECORATION: none}
        
        a:hover {FONT-WEIGHT: bolder; FONT-SIZE: 11px; COLOR: #e48625; FONT-FAMILY: Arial, Helvetica, sans-serif; TEXT-DECORATION: underline}
        
        a:active {FONT-WEIGHT: bold;FONT-SIZE: 11px COLOR: #e48625; FONT-FAMILY: Arial, Helvetica, sans-serif; TEXT-DECORATION: none}
        .tdName{width:180px}    
        .tdPC{width:30px}
        .tdPhoneNumber{width:80px}
        .tdExtension{width:30px}
        .tdMobile{width:74px}
        .tdManager{width:180px}
        .tdAddress{width:440px}
        .AltRow{background:#f5fffc}
    </style>
</head>
<body>
    <form id="form1" runat="server">
    <img src="images/MainTitleBgd.gif" id="MainTitleBgd" height="30px" style="position:absolute;Top:0px;left:0px;width:100%" />
    <img src="images/MainTitle.gif" style="position:absolute;top:0px;left:0px;margin-left:-5px" />
    
    <div id="Div2" style="position:absolute;top:4px;left:387px;color:white;font-weight:bold">
    <asp:CheckBox ID="CheckBox1" Text="View Ardmore extensions only" runat="server" AutoPostBack="True" />&nbsp;&nbsp;&nbsp;
    <asp:CheckBox ID="CheckBox2" Text="View Region Managers only" runat="server" AutoPostBack="True" />
    <span style="margin-left:30px">Search: <asp:TextBox ID="txtSearch" runat="server" style="font-size:8pt" size="20"></asp:TextBox></span></div>
    <div class="div1">
      <asp:Repeater ID="Repeater1" runat="server">
     <HeaderTemplate>
        <table>
            <tr>
            <th><asp:linkbutton ID="lnkOrderByName" runat=server CommandName="Name"  Text="Name"></asp:linkbutton></th>
            <th><asp:linkbutton ID="lnkOrderByPc" runat=server CommandName="PC"  Text="PC"></asp:linkbutton></th>
            <th><asp:linkbutton ID="Linkbutton0" runat=server CommandName="Number"  Text="Number"></asp:linkbutton></th>
            <th><asp:linkbutton ID="Linkbutton1" runat=server CommandName="Ext"  Text="Ext"></asp:linkbutton></th>
            <th><asp:linkbutton ID="Linkbutton2" runat=server CommandName="Mobile"  Text="Mobile"></asp:linkbutton></th>
            <th><asp:linkbutton ID="Linkbutton3" runat=server CommandName="Manager"  Text="Manager"></asp:linkbutton></th>
            <th><asp:linkbutton ID="Linkbutton4" runat=server CommandName="Address"  Text="Address"></asp:linkbutton></th>
            </tr>
     </HeaderTemplate>
     <ItemTemplate>
        <tr>
            <td class="tdName"><%#DataBinder.Eval(Container.DataItem, "Name")%></td>
            <td class="tdPC"><%#DataBinder.Eval(Container.DataItem, "pc")%></td>
            <td class="tdPhoneNumber"><a href="tt:<%#DataBinder.Eval(Container.DataItem,"PhoneNumber") %>?Dial"><%#DataBinder.Eval(Container.DataItem, "PhoneNumber")%></a></td>
            <td class="tdExtension"><a href="tt:<%#DataBinder.Eval(Container.DataItem,"Extension") %>?Dial" ><%#DataBinder.Eval(Container.DataItem, "Extension")%></a></td>
            <td class="tdMobile"><a href="tt:<%#DataBinder.Eval(Container.DataItem,"Mobile") %>?Dial" ><%#DataBinder.Eval(Container.DataItem, "Mobile")%></a></td>
            <td class="tdManager"><%#DataBinder.Eval(Container.DataItem, "ManagerName")%></td>
            <td class="tdAddress"><%#DataBinder.Eval(Container.DataItem, "Address")%></td>
        </tr>
     </ItemTemplate>
     <AlternatingItemTemplate>
        <tr class="AltRow">
            <td class="tdName"><%#DataBinder.Eval(Container.DataItem, "Name")%></td>
            <td class="tdPC"><%#DataBinder.Eval(Container.DataItem, "pc")%></td>
            <td class="tdPhoneNumber"><a href="tt:<%#DataBinder.Eval(Container.DataItem,"PhoneNumber") %>?Dial"><%#DataBinder.Eval(Container.DataItem, "PhoneNumber")%></a></td>
            <td class="tdExtension"><a href="tt:<%#DataBinder.Eval(Container.DataItem,"Extension") %>?Dial"><%#DataBinder.Eval(Container.DataItem, "Extension")%></a></td>
            <td class="tdMobile"><a href="tt:<%#DataBinder.Eval(Container.DataItem,"Mobile") %>?Dial" ><%#DataBinder.Eval(Container.DataItem, "Mobile")%></a></td>
            <td class="tdManager"><%#DataBinder.Eval(Container.DataItem, "ManagerName")%></td>
            <td class="tdAddress"><%#DataBinder.Eval(Container.DataItem, "Address")%></td>
        </tr>
     </AlternatingItemTemplate>
     <FooterTemplate>
        </table>
     </FooterTemplate>
    </asp:Repeater>
    </div>
    </form>
</body>
</html>
