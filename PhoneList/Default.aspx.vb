Imports System.Data
Partial Class _Default
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

        Dim oPhoneItem As PhoneItem
        Dim PhoneItems As List(Of PhoneItem)

        If Session("oPhoneItem") Is Nothing Then
            oPhoneItem = New PhoneItem
            oPhoneItem.LoadList()
            Session("oPhoneItem") = oPhoneItem
        Else
            oPhoneItem = Session("oPhoneItem")
        End If

        If Not IsPostBack Then
            PhoneItems = (From p In oPhoneItem.PhoneItems Order By p.Name).ToList
            Repeater1.DataSource = PhoneItems
            Repeater1.DataBind()
        End If

    End Sub

    Protected Function formatdata(ByVal Item As Object, ByVal FieldName As String) As String
        Dim DatumRow As DataRowView = Item
        Select Case FieldName
        End Select
    End Function

    Protected Sub txtSearch_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles txtSearch.TextChanged

        'If Trim(txtSearch.Text) = "" Then Exit Sub

        Dim oPhoneItem As PhoneItem = Session("oPhoneItem")
        Dim PhoneItems = oPhoneItem.GetSearchResults(UCase(txtSearch.Text))
        If CheckBox1.Checked Then PhoneItems = (From p In PhoneItems Where p.Extension <> "").ToList
        If CheckBox2.Checked Then PhoneItems = (From p In PhoneItems Where p.IsRegionManager = True).ToList

        Repeater1.DataSource = PhoneItems
        Repeater1.DataBind()

    End Sub

    Protected Sub Repeater1_ItemCommand(ByVal source As Object, ByVal e As System.Web.UI.WebControls.RepeaterCommandEventArgs) Handles Repeater1.ItemCommand

        Dim oPhoneItem As PhoneItem = Session("oPhoneItem")
        Dim PhoneItems As List(Of PhoneItem)

        If txtSearch.Text = "" Then
            PhoneItems = oPhoneItem.PhoneItems
        Else
            PhoneItems = oPhoneItem.GetSearchResults(UCase(txtSearch.Text))
        End If

        Dim WhereClause As String = ""

        Select Case e.CommandName
            Case "Name"
                PhoneItems = (From p In PhoneItems Order By p.Name).ToList
            Case "PC"
                PhoneItems = (From p In PhoneItems Order By p.PC, p.Name).ToList
            Case "Number"
                PhoneItems = (From p In PhoneItems Order By p.PhoneNumber, p.Name).ToList
            Case "Ext"
                PhoneItems = (From p In PhoneItems Order By p.Extension, p.Name).ToList
            Case "Mobile"
                PhoneItems = (From p In PhoneItems Order By p.Mobile, p.Name).ToList
            Case "Manager"
                PhoneItems = (From p In PhoneItems Order By p.ManagerName, p.Name).ToList
            Case "Address"
                PhoneItems = (From p In PhoneItems Order By p.Address, p.Name).ToList

        End Select
        If CheckBox1.Checked Then PhoneItems = (From p In PhoneItems Where p.Extension <> "").ToList
        If CheckBox2.Checked Then PhoneItems = (From p In PhoneItems Where p.IsRegionManager = True).ToList

        Repeater1.DataSource = PhoneItems
        Repeater1.DataBind()

    End Sub

    Protected Sub CheckBox1_CheckedChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles CheckBox1.CheckedChanged
        'View Ardmore extensions only.

        Dim PhoneItems As List(Of PhoneItem)
        Dim oPhoneItem As PhoneItem = Session("oPhoneItem")
        If CheckBox1.Checked Then

            If txtSearch.Text = "" Then
                PhoneItems = (From p In oPhoneItem.PhoneItems Order By p.Name Where p.Extension <> "").ToList
            Else
                PhoneItems = oPhoneItem.GetSearchResults(UCase(txtSearch.Text))
                PhoneItems = (From p In PhoneItems Order By p.Name Where p.Extension <> "").ToList
            End If

        Else
            If txtSearch.Text = "" Then
                PhoneItems = (From p In oPhoneItem.PhoneItems Order By p.Name).ToList
            Else
                PhoneItems = oPhoneItem.GetSearchResults(UCase(txtSearch.Text))
                PhoneItems = (From p In PhoneItems Order By p.Name).ToList
            End If
        End If
        Repeater1.DataSource = PhoneItems
        Repeater1.DataBind()

        CheckBox2.Checked = False
    End Sub

    Protected Sub CheckBox2_CheckedChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles CheckBox2.CheckedChanged
        'View region managers only.

        Dim PhoneItems As List(Of PhoneItem)
        Dim oPhoneItem As PhoneItem = Session("oPhoneItem")
        If CheckBox2.Checked Then

            If txtSearch.Text = "" Then
                PhoneItems = (From p In oPhoneItem.PhoneItems Order By p.Name Where p.IsRegionManager = True).ToList
            Else
                PhoneItems = oPhoneItem.GetSearchResults(UCase(txtSearch.Text))
                PhoneItems = (From p In PhoneItems Order By p.Name Where p.IsRegionManager = True).ToList
            End If

        Else
            If txtSearch.Text = "" Then
                PhoneItems = (From p In oPhoneItem.PhoneItems Order By p.Name).ToList
            Else
                PhoneItems = oPhoneItem.GetSearchResults(UCase(txtSearch.Text))
                PhoneItems = (From p In PhoneItems Order By p.Name).ToList
            End If
        End If
        Repeater1.DataSource = PhoneItems
        Repeater1.DataBind()

        CheckBox1.Checked = False
    End Sub
End Class
