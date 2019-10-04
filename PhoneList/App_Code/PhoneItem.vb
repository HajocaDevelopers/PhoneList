Imports Microsoft.VisualBasic
Imports HajClassLib
Imports System.Data
Imports System.IO

Imports System.Linq
Imports System.Collections.Generic




Public Class PhoneItem
    Private _PhoneNumber As String
    Public ReadOnly Property PhoneNumber() As String
        Get
            Return _PhoneNumber
        End Get
    End Property

    Private _Extension As String
    Public ReadOnly Property Extension() As String
        Get
            Return _Extension
        End Get
    End Property

    Private _Mobile As String
    Public ReadOnly Property Mobile() As String
        Get
            Return _Mobile
        End Get
    End Property

    Private _PC As String
    Public ReadOnly Property PC() As String
        Get
            Return _PC
        End Get
    End Property

    Private _Name As String
    Public ReadOnly Property Name() As String
        Get
            Return _Name
        End Get
    End Property

    Private _ManagerName As String
    Public ReadOnly Property ManagerName() As String
        Get
            Return _ManagerName
        End Get
    End Property

    Private _Address As String
    Public ReadOnly Property Address() As String
        Get
            Return _Address
        End Get
    End Property

    Private _IsRegionManager As Boolean
    Public ReadOnly Property IsRegionManager() As Boolean
        Get
            Return _IsRegionManager
        End Get
    End Property

    Public PhoneItems As New List(Of PhoneItem)

    Public Sub New()

    End Sub
    Public Sub LoadList()

        LoadPcInfo()
        LoadPcOverrides()
        LoadPeople()
    End Sub


    Public Function GetSearchResults(ByVal SearchText As String) As List(Of PhoneItem)

        SearchText = Trim(SearchText)

        Dim Results = (From i In PhoneItems _
                      Where UCase(i.Name).Contains(SearchText) _
                          Or (i.PhoneNumber IsNot Nothing AndAlso i.PhoneNumber.Contains(SearchText)) _
                          Or (i.Extension IsNot Nothing AndAlso i.Extension.Contains(SearchText)) _
                          Or (i.Mobile IsNot Nothing AndAlso i.Mobile.Contains(SearchText)) _
                          Or (i.PC IsNot Nothing AndAlso i.PC.Contains(SearchText)) _
                          Or (i.ManagerName IsNot Nothing AndAlso i.ManagerName.Contains(SearchText)) _
                          Or (i.Address IsNot Nothing AndAlso i.Address.Contains(SearchText)) Order By Name).ToList

        Return Results
    End Function

    Sub LoadPcInfo()
        Dim oHajProfitCenter As New HajProfitCenter
        Dim dt As DataTable = oHajProfitCenter.GetTable("select pc,pcname,pcphon,pcmgr,pcadd1,pcadd2,pccity,pcst,pcpozp", " where pcclos=0")
        For Each rw In dt.Rows
             If rw("pcphon") <> 0 Then
                Dim oPhoneItem As New PhoneItem
                With oPhoneItem
                    ._PhoneNumber = CLng(rw("pcphon")).ToString("000-000-0000")

                    ._PC = CInt(rw("pc")).ToString("000")

                    ._Name = rw("pcname")
                    ._ManagerName = Replace(rw("pcmgr"), "- MGR", "")
                    ._ManagerName = Replace(._ManagerName, "-MGR", "")

                    Dim Addr1 As String = rw("pcadd1")
                    Dim Addr2 As String = rw("pcadd2")
                    Dim City As String = rw("pccity")
                    Dim State As String = rw("pcst")
                    Dim Zip As String = rw("pcpozp")

                    If rw("pcpozp") = 0 Then
                        Zip = ""
                    ElseIf Mid(Zip, 6) = "0000" Or Mid(Zip, 6) = "000" Then
                        Zip = Mid(Zip, 1, 5)
                    Else
                        Zip = CLng(Zip).ToString("00000-0000")
                    End If

                    If Addr2 = "" Then
                        ._Address = Addr1 & ", " & City & ", " & State & " " & Zip
                    Else
                        ._Address = Addr1 & ", " & Addr2 & ", " & City & ", " & State & " " & Zip

                    End If
                    If Mid(._Address, 1, 1) = "," Then ._Address = Mid(._Address, 2)
                End With
                PhoneItems.Add(oPhoneItem)

            End If
        Next

    End Sub

    Sub LoadAddendum()
        'These are phone numbers that aren't in the pc master.

    End Sub

    Sub LoadPcOverrides()
        Dim sr As StreamReader
        Try
            sr = File.OpenText("\\hajfp1\phonelist\PcOverrides.txt")
        Catch ex As Exception
            sendMail("Unable to open \\hajfp1\phonelist\PcOverrides.txt" & vbCrLf & ex.Message & "..." & ex.StackTrace)
        End Try

        sr.ReadLine()
        Do Until sr.EndOfStream
            Dim tags() As String = Split(sr.ReadLine, vbTab)
            If tags(0) <> "" Then
                Dim myPC As Integer = tags(0)
                Dim oPhoneItem As List(Of PhoneItem) = (From p In PhoneItems Where p.PC = myPC).ToList
                oPhoneItem(0)._PhoneNumber = tags(1)
            End If
        Loop

        sr.Close()

    End Sub


    Sub LoadPeople()
        Try
            Dim sr As StreamReader
            Try
                sr = File.OpenText("\\hajfp1\phonelist\PersonalPhoneList.txt")
            Catch ex As Exception
                sendMail("Unable to open \\hajfp1\phonelist\PersonalPhoneList.txt" & vbCrLf & ex.Message & "..." & ex.StackTrace)
            End Try
            sr.ReadLine()
            sr.ReadLine()
            Do Until sr.EndOfStream
                Dim tags() As String = Split(sr.ReadLine, vbTab)
                Dim HasNumber As Boolean = False
                If tags(0) <> "" Then
                    Dim oPhoneItem As New PhoneItem
                    With oPhoneItem

                        If tags.Length >= 1 AndAlso Trim(tags(0)) <> "" Then
                            ._Name = tags(0)
                            ._Name = Replace(._Name, "’", "'")
                            If Mid(._Name, 1, 3) = "Joe" Then
                                Dim c As String = Mid(._Name, 6, 1)
                                If Asc(Mid(._Name, 6, 1)) = 63 Then
                                    ._Name = Replace(._Name, Mid(._Name, 6, 1), "'")
                                End If
                            End If
                        End If

                        If tags.Length >= 2 AndAlso Trim(tags(1)) <> "" Then
                            If IsNumeric(tags(1)) Then
                                ._PhoneNumber = CLng(tags(1)).ToString("000-000-0000")
                                HasNumber = True
                            End If
                        End If

                        If tags.Length >= 3 AndAlso Trim(tags(2)) <> "" Then
                            ._Extension = tags(2)
                            HasNumber = True
                        End If

                        If tags.Length >= 4 AndAlso Trim(tags(3)) <> "" Then
                            If IsNumeric(tags(3)) Then
                                ._Mobile = CLng(tags(3)).ToString("000-000-0000")
                                HasNumber = True
                            End If
                        End If

                        If tags.Length >= 5 AndAlso UCase(Trim(tags(4))) = "X" Then
                            ._IsRegionManager = True
                        End If

                    End With
                    If HasNumber Then PhoneItems.Add(oPhoneItem)
                End If
            Loop

            sr.Close()
        Catch ex As Exception
            sendMail(ex.Message)
        End Try

    End Sub


    Private Sub sendMail(ByVal Mesg As String)

        Dim oMail As New HajClassLib.Mailer
        oMail.MailTo.Add("dpratt@hajoca.com")
        oMail.Body = Mesg & vbCrLf & "UserId=" & HttpContext.Current.User.Identity.Name
        oMail.Subject = "Error in Phonelist"
        oMail.Send()


    End Sub
End Class
