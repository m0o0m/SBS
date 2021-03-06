﻿Imports System.Collections.Generic
Imports System.Linq
Imports SBCBL.CacheUtils
Imports SBCBL.std
Imports SBCBL.Tickets
Imports SBCBL.Managers
Imports System.Collections
Imports System.Data
Partial Class SBS_Agents_Inc_Game_SelectGame
    Inherits SBCBL.UI.CSBCUserControl
    Private _sGameType As String = SBCBL.std.GetSiteType & " GameType"
    Private _SuperBookmakerValue As String = SBCBL.std.GetSiteType & " BookmakerType"
    Private _sOffTimeCategory As String = SBCBL.std.GetSiteType & " LineOffHour"
    Private _bSelectedPropGame As Boolean = False
    Private _bHavePropGame As Boolean = False
    Private _Straight As String = "Straight"
    Private _BetIfAll As String = "BetIfAll"
    Private _BetTheBoard As String = "BetTheBoard"
    Dim sCategory As String = "TeaserAllow"
    Private _log As log4net.ILog = log4net.LogManager.GetLogger(Me.GetType())

    Private Property Availablegame() As Dictionary(Of String, Boolean)
        Get
            If ViewState("Availablegame") Is Nothing Then
                ViewState("Availablegame") = New Dictionary(Of String, Boolean)
            End If
            Return CType(ViewState("Availablegame"), Dictionary(Of String, Boolean))
        End Get
        Set(ByVal value As Dictionary(Of String, Boolean))
            ViewState("Availablegame") = value
        End Set
    End Property

    Private Property AvailablegamePart() As Dictionary(Of String, Integer())
        Get
            If ViewState("AvailablegamePart") Is Nothing Then
                ViewState("AvailablegamePart") = New Dictionary(Of String, Integer())
            End If
            Return CType(ViewState("AvailablegamePart"), Dictionary(Of String, Integer()))
        End Get
        Set(ByVal value As Dictionary(Of String, Integer()))
            ViewState("AvailablegamePart") = value
        End Set
    End Property

    Public Property BetActionLink() As String
        Get
            Return SafeString(ViewState("BetActionLink"))
        End Get
        Set(ByVal value As String)
            ViewState("BetActionLink") = value
        End Set
    End Property

    Public ReadOnly Property SelectedPlayer() As CPlayer
        Get
            If UserSession.UserType = SBCBL.EUserType.Player Then
                Return UserSession.PlayerUserInfo
            Else
                If Not String.IsNullOrEmpty(SelectedPlayerID) Then
                    Return UserSession.Cache.GetPlayerInfo(SelectedPlayerID)
                Else
                    Return Nothing
                End If

            End If
        End Get
    End Property

    Public ReadOnly Property BetType() As String
        Get
            Return SafeString(Request.QueryString("bettype"))
        End Get
    End Property

    Public Property SelectedPlayerID() As String
        Get
            If ViewState("_SELECTED_PLAYER_ID") Is Nothing Then
                If UserSession.UserType = SBCBL.EUserType.Player Then
                    ViewState("_SELECTED_PLAYER_ID") = UserSession.UserID
                End If
            End If
            ' Return "298EE9B3-0678-4519-9DAA-BA8A6A787B36"
            Return SafeString(ViewState("_SELECTED_PLAYER_ID"))
        End Get
        Set(ByVal value As String)
            ViewState("_SELECTED_PLAYER_ID") = value
            'ucWagers.SelectedPlayerID = value
        End Set
    End Property

    '' set super Agent Id from call center
    Public Property CallCenterAgentID() As String
        Get
            Return SafeString(ViewState("_CALLCENTER_AGENT_ID"))
        End Get
        Set(ByVal value As String)
            ViewState("_CALLCENTER_AGENT_ID") = value
        End Set
    End Property

    'Public ReadOnly Property ContextFilterQuery(ByVal psGameType As String, ByVal psContext As String) As String
    '    Get
    '        Dim sKey = "ContextFilterQuery_" & psGameType & "_" & psContext
    '        If Session(sKey) Is Nothing Then
    '            Session(sKey) = CreateContextFilterQuery(psGameType, psContext)
    '        End If
    '        Return Session(sKey).ToString()
    '    End Get
    'End Property

    Public Property Game() As Dictionary(Of String, CGame)
        Get
            If ViewState("Game") Is Nothing Then
                ViewState("Game") = New Dictionary(Of String, CGame)
            End If
            Return CType(ViewState("Game"), Dictionary(Of String, CGame))
        End Get
        Set(ByVal value As Dictionary(Of String, CGame))

            ViewState("Game") = value
        End Set
    End Property

    Public Property GameSelect() As Dictionary(Of String, CGame)
        Get
            If Session("GameSelect") Is Nothing Then
                Session("GameSelect") = New Dictionary(Of String, CGame)
            End If
            Return CType(Session("GameSelect"), Dictionary(Of String, CGame))
        End Get
        Set(ByVal value As Dictionary(Of String, CGame))

            Session("GameSelect") = value
        End Set
    End Property

    Public ReadOnly Property AgentCategory() As String
        Get
            If UserSession.UserType = SBCBL.EUserType.Agent AndAlso UserSession.AgentUserInfo.IsSuperAgent Then
                Return UserSession.UserID + " SportAllow"
            End If
            Return SuperAgentId + " SportAllow"
        End Get
    End Property

    Public ReadOnly Property AgentgameTypeDisaple() As List(Of String)
        Get
            Dim oMainGameTypeDisable As New List(Of String)
            Dim oMainGameType As List(Of SBCBL.CacheUtils.CSysSetting)
            Dim oCacheManager As New CCacheManager()

            oMainGameType = oCacheManager.GetAllSysSettings(AgentCategory)
            If oMainGameType.Count > 0 Then
                oMainGameTypeDisable = (From oSystemSetting As SBCBL.CacheUtils.CSysSetting In oMainGameType Where oSystemSetting.Value.Equals("No", StringComparison.CurrentCultureIgnoreCase) Select oSystemSetting.Key).ToList
            Else
                oMainGameType = oCacheManager.GetAllSysSettings(_sGameType)
                oMainGameTypeDisable = (From oSystemSetting As SBCBL.CacheUtils.CSysSetting In oMainGameType Where oSystemSetting.Value.Equals("No", StringComparison.CurrentCultureIgnoreCase) Select oSystemSetting.Key).ToList
            End If
            Return oMainGameTypeDisable
        End Get
    End Property

    Public ReadOnly Property SuperAgentId() As String
        Get
            If UserSession.UserType = SBCBL.EUserType.Agent Then
                Return UserSession.AgentUserInfo.SuperAgentID
            ElseIf UserSession.UserType = SBCBL.EUserType.Player Then
                Return UserSession.PlayerUserInfo.SuperAgentID
            Else
                Return CallCenterAgentID
            End If
        End Get
    End Property


    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

        If Not Me.IsPostBack Then

            If LCase(BetType).Contains("if ") Then
                Dim nIndex As Integer = 1
                If UserSession.SelectedTicket(SelectedPlayerID) IsNot Nothing Then
                    nIndex = UserSession.SelectedTicket(SelectedPlayerID).Tickets.Count + 1
                End If

                ' lblSelectGameMsg.Text = String.Format("Select a sport for the <b>{0}</b> selection of the if-bet.", GetOrdinalNumber(nIndex))
            Else
                'lblSelectGameMsg.Text = String.Format("Select one or more sports for your {0}.", BetType.Replace("BetTheBoard", "Straight Bet(s)"))
            End If

            Session("BetTypeProp") = False
            ' lblBetType.Text = BetType.Replace("Reverse", "Action Reverse").Replace("BetTheBoard", "Straight Bet(s)")
            If UCase(BetType) = "TEASER" Then
                TeaserType.Visible = True
                divTeaserType.Visible = True
                ' rptGameTypeShort.Visible = False
                divrptGameTypeShort.Visible = False

                pnSelectGame.Visible = False '.Style.Add("display", "none")
                dvPropGame.Visible = False
                Dim oSysManager As New CSysSettingManager()
                Dim oListSettings As CSysSettingList = UserSession.SysSettings(sCategory)
                If oListSettings IsNot Nothing Then
                    Dim strAgent As String = SuperAgentId
                    Dim sKey As String = strAgent & "4/6"
                    Dim oSetting As CSysSetting = oListSettings.Find(Function(x) x.Key = sKey)
                    If oSetting IsNot Nothing Then
                        tr46.Visible = String.IsNullOrEmpty(oSetting.Value) OrElse SafeBoolean(oSetting.Value)
                    End If
                    sKey = strAgent & "4.5/6.5"
                    oSetting = oListSettings.Find(Function(x) x.Key = sKey)
                    If oSetting IsNot Nothing Then
                        tr4565.Visible = String.IsNullOrEmpty(oSetting.Value) OrElse SafeBoolean(oSetting.Value)
                    End If
                    sKey = strAgent & "5/7"
                    oSetting = oListSettings.Find(Function(x) x.Key = sKey)
                    If oSetting IsNot Nothing Then
                        tr57.Visible = String.IsNullOrEmpty(oSetting.Value) OrElse SafeBoolean(oSetting.Value)
                    End If
                    sKey = strAgent & "5.5/7.5"
                    oSetting = oListSettings.Find(Function(x) x.Key = sKey)
                    If oSetting IsNot Nothing Then
                        tr5575.Visible = String.IsNullOrEmpty(oSetting.Value) OrElse SafeBoolean(oSetting.Value)
                    End If
                    sKey = strAgent & "3/8"
                    oSetting = oListSettings.Find(Function(x) x.Key = sKey)
                    If oSetting IsNot Nothing Then
                        tr38.Visible = String.IsNullOrEmpty(oSetting.Value) OrElse SafeBoolean(oSetting.Value)
                    End If
                    sKey = strAgent & "4/13"
                    oSetting = oListSettings.Find(Function(x) x.Key = sKey)
                    If oSetting IsNot Nothing Then
                        tr413.Visible = String.IsNullOrEmpty(oSetting.Value) OrElse SafeBoolean(oSetting.Value)
                    End If
                End If
            ElseIf UCase(BetType) = "PROP" Then
                TeaserType.Visible = False
                divTeaserType.Visible = False
                'rptGameTypeShort.Visible = False
                divrptGameTypeShort.Visible = False


                pnSelectGame.Visible = False '.Style.Add("display", "none")
                dvPropGame.Visible = True
            Else

                TeaserType.Visible = False
                divTeaserType.Visible = False
                pnSelectGame.Visible = True '.Style.Add("display", "")
                dvPropGame.Visible = False
            End If

            ' bindSuperInfo()
            If UserSession.UserType = SBCBL.EUserType.Agent Then
                BindSubAgents()
                BindPlayers()
                dvPlayer.Visible = True
                GameSelect.Clear()
                lbtAgentContinue.Text = "<a href='#' onclick='javascript:CheckEmptyUser(this)' >Continue</a>"
                btnContinue.Visible = False
            Else
                lbtAgentContinue.Visible = False
                btnContinue.Visible = True
            End If

            ' If Not String.IsNullOrEmpty(SelectedPlayerID) Then
            'Dim oCacheManager As CCacheManager = New CCacheManager()
            'DisplayInfo()
            ' ltlLoginID.Text = oCacheManager.GetPlayerInfo(SelectedPlayerID).Login
            ' ltlBalance.Text = FormatNumber(oCacheManager.GetPlayerInfo(SelectedPlayerID).BalanceAmount, SBCBL.std.GetRoundMidPoint())
            ' End If

            'If BetType.Equals(_BetTheBoard, StringComparison.CurrentCultureIgnoreCase) Then
            '    bettypeTitle.Visible = False
            '    lblBetTheBoard.Text = "Display the board by selecting one or more sports below.."
            'End If


            bindGameTypes()




        End If

    End Sub

    'Public Sub DisplayInfo()
    '    If Not String.IsNullOrEmpty(SelectedPlayerID) Then
    '        Dim oCacheManager As CCacheManager = New CCacheManager()
    '        ltlBalance.Text = FormatNumber(oCacheManager.GetPlayerInfo(SelectedPlayerID).BalanceAmount, SBCBL.std.GetRoundMidPoint())
    '        ltlCreditLimit.Text = FormatNumber(oCacheManager.GetPlayerInfo(SelectedPlayerID).Template.CreditMaxAmount, SBCBL.std.GetRoundMidPoint())
    '        ltlPendingAmount.Text = FormatNumber(oCacheManager.GetPlayerInfo(SelectedPlayerID).PendingAmount, SBCBL.std.GetRoundMidPoint())

    '    End If
    'End Sub

    Private Sub bindSuperInfo()
        Dim oSuperManager As New CSuperUserManager()
        Dim odtSuperAdmin As New DataTable
        If UserSession.UserType = SBCBL.EUserType.Player Then
            odtSuperAdmin = oSuperManager.GetSuperByID(UserSession.PlayerUserInfo.SuperAdminID)
        End If
        If UserSession.UserType = SBCBL.EUserType.Agent Then
            odtSuperAdmin = oSuperManager.GetSuperByID(UserSession.AgentUserInfo.SuperAdminID)
        End If
        If UserSession.UserType = SBCBL.EUserType.CallCenterAgent Then
            Dim oCacheManager As CCacheManager = New CCacheManager()
            odtSuperAdmin = oSuperManager.GetSuperByID(oCacheManager.GetPlayerInfo(UserSession.CCAgentUserInfo.PlayerID).SuperAdminID)
        End If
        'If Not String.IsNullOrEmpty(SafeString(odtSuperAdmin.Rows(0)("Wagering"))) Then
        '    lblWagering.Text = " <strong>Wagering #:</strong> " & SafeString(odtSuperAdmin.Rows(0)("Wagering")) & "<br/>"
        'End If

        'If Not String.IsNullOrEmpty(SafeString(odtSuperAdmin.Rows(0)("CustomerService"))) Then
        '    lblCustomerService.Text = " <strong>Customer Service #:</strong>" & SafeString(odtSuperAdmin.Rows(0)("CustomerService"))
        'End If

    End Sub

    Private Sub BindSubAgents()

        Dim oMng As New SBCBL.Managers.CAgentManager()
        ddlSubAgents.DataSource = oMng.GetAgentsByAgentID(UserSession.UserID, Nothing)
        ddlSubAgents.DataValueField = "AgentID"
        ddlSubAgents.DataTextField = "FullName"
        ddlSubAgents.DataBind()
        wager.Style.Add("padding-top", "25px")
        dvSubAgents.Visible = ddlSubAgents.Items.Count > 1
    End Sub

    <System.Web.Services.WebMethod()> _
    Private Sub BindPlayers()

        Dim oMng As New SBCBL.Managers.CPlayerManager()
        If ddlSubAgents.Value <> "" Then
            ddlPlayers.DataSource = oMng.GetPlayers(ddlSubAgents.Value, Nothing)
        Else
            ddlPlayers.DataSource = oMng.GetPlayers(UserSession.UserID, Nothing)
        End If

        ddlPlayers.DataValueField = "PlayerID"
        ddlPlayers.DataTextField = "FullName"
        ddlPlayers.DataBind()
    End Sub

    Protected Sub ddlSubAgents_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles ddlSubAgents.SelectedIndexChanged
        BindPlayers()
    End Sub

#Region "Game Types"

    Private Sub bindGameTypes()

        Dim lstSportTypes As New List(Of String)
        lstSportTypes.Add("Live Game")
        lstSportTypes.Add("Football")
        lstSportTypes.Add("Basketball")
        lstSportTypes.Add("Baseball")
        lstSportTypes.Add("Hockey")
        lstSportTypes.Add("Soccer")
        lstSportTypes.Add("Other Sports")


        Dim lstGameType As New List(Of String)

        lstGameType.AddRange(getListGameType("Live Game"))
        lstGameType.AddRange(getListGameType("Football"))
        lstGameType.AddRange(getListGameType("Basketball"))
        lstGameType.AddRange(getListGameType("Baseball"))
        lstGameType.AddRange(getListGameType("Hockey"))
        Dim lstSoccer As List(Of String) = getListGameType("Soccer")
        lstSoccer.Sort()
        lstGameType.AddRange(lstSoccer)
        lstGameType.AddRange(getListGameType("Other Sports"))

        Dim odtAvailablegame As DataTable = GetAvailableGameType(lstGameType)
        If odtAvailablegame IsNot Nothing Then
            For Each odr As DataRow In odtAvailablegame.Rows
                Dim oContext As Integer() = {SafeInteger(odr("FullGame")), SafeInteger(odr("FirstHalf")), SafeInteger(odr("SecondHalf")), SafeInteger(odr("Quarter"))}
                AvailablegamePart(SafeString(odr("GameType"))) = oContext
                If SafeInteger(odr("FullGame")) > 0 OrElse SafeInteger(odr("FirstHalf")) > 0 OrElse SafeInteger(odr("SecondHalf")) > 0 OrElse SafeInteger(odr("Quarter")) > 0 Then
                    Availablegame(SafeString(odr("GameType"))) = True
                Else
                    Availablegame(SafeString(odr("GameType"))) = False
                End If

            Next
        End If
        ''end
        rptGameType.DataSource = lstSportTypes ' lstGameTypes
        rptGameType.DataBind()
        'rptGameTypeShort.DataSource = lstSportTypes ' lstGameTypes
        'rptGameTypeShort.DataBind()

        Dim oGameTypes As List(Of CSysSetting)
        Dim oSysProp = UserSession.Cache.GetAllSysSettings(_sGameType).Find(Function(x) x.Key = "Proposition")
        Dim lstPropGameType As New List(Of String)
        If oSysProp IsNot Nothing Then
            oGameTypes = UserSession.Cache.GetAllSysSettings(_sGameType).FindAll(Function(x) x.SubCategory = oSysProp.SysSettingID And UCase(x.Value) = "YES" And Not AgentgameTypeDisaple.Contains(x.Key))
            lstPropGameType = (From oGameType In oGameTypes Select oGameType.Key).ToList()
            Dim odtGames As DataTable = New SBCBL.Managers.CGameManager().GetGameTypeAvailableProps(GetEasternDate(), lstPropGameType)
            Dim lstPropgamTypeAvailable As New List(Of String)
            If odtGames IsNot Nothing Then


                For Each oRow As DataRow In odtGames.Rows

                    lstPropgamTypeAvailable.Add(SafeString(oRow("GameType")))
                Next
                rptPropGame.DataSource = lstPropgamTypeAvailable
                rptPropGame.DataBind()

                If lstPropgamTypeAvailable.Count = 0 Then
                    dvPropGame.Visible = False
                    'Else
                    '    dvPropGame.Visible = True
                End If

            End If
        End If
    End Sub



    Public Function GetBookMakerType(ByVal psKey As String, ByVal psAgentID As String, ByVal psBookMakerType As String) As String
        Dim sAgentID As String = psAgentID
        Dim sBookMakerType As String = psBookMakerType
        While True
            Dim sBookMakerValue As String = UserSession.Cache.GetSysSettings(sBookMakerType).GetValue(psKey)
            If Not String.IsNullOrEmpty(sBookMakerValue) Then
                Return sBookMakerType
            End If
            If Not String.IsNullOrEmpty(UserSession.Cache.GetAgentInfo(sAgentID).ParentID) Then
                sAgentID = UserSession.Cache.GetAgentInfo(sAgentID).ParentID
                sBookMakerType = sAgentID + "_BookmakerType"
            Else
                Return _SuperBookmakerValue
            End If
        End While

        Return _SuperBookmakerValue
    End Function

    ''get BookMaker for load game
    Private Function GetAgentBookMaker(ByVal psGameType As String) As String
        If UserSession.UserType = SBCBL.EUserType.Agent Then
            Return GetBookMakerType(psGameType, UserSession.UserID, SafeString(UserSession.UserID & "_BookmakerType"))
        End If
        If UserSession.UserType = SBCBL.EUserType.Player Then
            Return GetBookMakerType(psGameType, UserSession.PlayerUserInfo.AgentID, SafeString(UserSession.PlayerUserInfo.AgentID & "_BookmakerType"))
        End If
        If UserSession.UserType = SBCBL.EUserType.CallCenterAgent Then
            Return GetBookMakerType(psGameType, UserSession.Cache.GetPlayerInfo(UserSession.CCAgentUserInfo.PlayerID).AgentID, SafeString(UserSession.Cache.GetPlayerInfo(UserSession.CCAgentUserInfo.PlayerID).AgentID & "_BookmakerType"))

        End If

        Return "Pinnacle2"
    End Function
    Private Shared Function SortGameType( _
        ByVal x As String, ByVal y As String) As Boolean

        Return String.Compare(x, y)

    End Function

    Protected Sub rptGameType_ItemDataBound(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.RepeaterItemEventArgs) Handles rptGameType.ItemDataBound
        If e.Item.ItemType = ListItemType.AlternatingItem OrElse e.Item.ItemType = ListItemType.Item Then

            Dim sSportType As String = CType(e.Item.DataItem, String)


            Dim lstGameType As List(Of String) = getListGameType(sSportType)
            For Each StrGameDis As String In AgentgameTypeDisaple
                If lstGameType.Contains(StrGameDis) Then
                    lstGameType.Remove(StrGameDis)
                End If
            Next

            If lstGameType IsNot Nothing And lstGameType.Count > 0 Then
                Dim rptSubGameType As Repeater = CType(e.Item.FindControl("rptSubGameType"), Repeater)
                If sSportType.Equals("Soccer", StringComparison.CurrentCultureIgnoreCase) Then
                    lstGameType.Sort()
                End If
                rptSubGameType.DataSource = lstGameType
                rptSubGameType.DataBind()
            Else
                e.Item.Visible = False
            End If
        End If
    End Sub



    Protected Sub rptSubGameType_ItemDataBound(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.RepeaterItemEventArgs)
        If e.Item.ItemType = ListItemType.Item OrElse e.Item.ItemType = ListItemType.AlternatingItem Then

            Dim sGameType As String = SafeString(e.Item.DataItem)

            If IsBaseball(sGameType) Then
                CType(e.Item.FindControl("lblOneH"), Label).Text = "1st 5 innings"

            Else
                CType(e.Item.FindControl("lblOneH"), Label).Text = "1st Half"
                CType(e.Item.FindControl("lblTwoH"), Label).Text = "2nd Half"
            End If
            Dim sBookmaker As String = UserSession.SysSettings(GetAgentBookMaker(sGameType)).GetValue(sGameType)
            Dim chkOneH, chkTwoH, chkCurrent, chkQuarter1, chkQuarter2, chkQuarter3, chkQuarter4 As CheckBox
            Dim liOneH, liTwoH, liQuarter1, liQuarter2, liQuarter3, liQuarter4, lichkCurrent As HtmlControl

            lichkCurrent = CType(e.Item.FindControl("lichkCurrent"), HtmlControl)

            liOneH = CType(e.Item.FindControl("liOneH"), HtmlControl)
            liTwoH = CType(e.Item.FindControl("liTwoH"), HtmlControl)
            'liCurrent = CType(e.Item.FindControl("liCurrent"), HtmlControl)
            liQuarter1 = CType(e.Item.FindControl("liQuarter1"), HtmlControl)
            liQuarter2 = CType(e.Item.FindControl("liQuarter2"), HtmlControl)
            liQuarter3 = CType(e.Item.FindControl("liQuarter3"), HtmlControl)
            liQuarter4 = CType(e.Item.FindControl("liQuarter4"), HtmlControl)

            liQuarter1.Visible = False

            liOneH.Visible = False
            liTwoH.Visible = False


            Dim lblOneH, lblTwoH, lblQuarter1, lblQuarter2, lblQuarter3, lblQuarter4, lblGameType As Label
            Dim lblGame As Literal

            chkQuarter1 = CType(e.Item.FindControl("chkQuarter1"), CheckBox)

            chkOneH = CType(e.Item.FindControl("chkOneH"), CheckBox)
            chkTwoH = CType(e.Item.FindControl("chkTwoH"), CheckBox)
            chkCurrent = CType(e.Item.FindControl("chkCurrent"), CheckBox)
            lblOneH = CType(e.Item.FindControl("lblOneH"), Label)
            lblTwoH = CType(e.Item.FindControl("lblTwoH"), Label)
            lblGameType = CType(e.Item.FindControl("lblGameType"), Label)

            lblQuarter1 = CType(e.Item.FindControl("lblQuarter1"), Label)
            lblQuarter2 = CType(e.Item.FindControl("lblQuarter2"), Label)
            lblQuarter3 = CType(e.Item.FindControl("lblQuarter3"), Label)
            lblQuarter4 = CType(e.Item.FindControl("lblQuarter4"), Label)
            lblGame = CType(e.Item.FindControl("lblGame"), Literal)
            Dim game = e.Item.FindControl("game")

            liOneH.Visible = False
            chkOneH.Visible = False

            liTwoH.Visible = False
            chkTwoH.Visible = False

            lblOneH.Visible = False
            lblTwoH.Visible = False
            lichkCurrent.Visible = False

            lblQuarter1.Visible = False


            liQuarter1.Visible = False
            chkQuarter1.Visible = False

            liQuarter2.Visible = False

            liQuarter3.Visible = False

            liQuarter4.Visible = False

            lblOneH.CssClass = ""
            lblTwoH.CssClass = ""
            lblGameType.CssClass = ""
            lblQuarter1.CssClass = ""

            If UserSession.Cache.GetSysSettings(SuperAgentId & "PartTimeDisplay", sGameType) IsNot Nothing Then

                If AvailablegamePart.ContainsKey(sGameType) AndAlso Not UserSession.Cache.GetSysSettings(SuperAgentId & "PartTimeDisplay", sGameType).GetBooleanValue("FirstHOff") AndAlso AvailablegamePart(sGameType)(1) And Not UserSession.FirstHalfOff(sGameType) Then
                    chkOneH.Visible = True
                    lblOneH.Visible = True
                    liOneH.Visible = True

                    game.Visible = True

                End If
                If AvailablegamePart.ContainsKey(sGameType) AndAlso Not UserSession.Cache.GetSysSettings(SuperAgentId & "PartTimeDisplay", sGameType).GetBooleanValue("SecondHOff") AndAlso AvailablegamePart(sGameType)(2) And Not UserSession.IsSecondhalfOff(sGameType) Then
                    chkTwoH.Visible = True
                    lblTwoH.Visible = True

                    liTwoH.Visible = True

                    game.Visible = True

                End If
            Else
                If AvailablegamePart.ContainsKey(sGameType) AndAlso AvailablegamePart(sGameType)(1) Then
                    chkOneH.Visible = True
                    lblOneH.Visible = True

                    liOneH.Visible = True

                    game.Visible = True

                End If
                If AvailablegamePart.ContainsKey(sGameType) AndAlso AvailablegamePart(sGameType)(2) Then
                    chkTwoH.Visible = True
                    lblTwoH.Visible = True

                    liTwoH.Visible = True

                    game.Visible = True

                End If
            End If

            If AvailablegamePart.ContainsKey(sGameType) AndAlso AvailablegamePart(sGameType)(0) Then
                chkCurrent.Visible = True

                lichkCurrent.Visible = True

                lblGameType.Visible = True

                game.Visible = True

            End If

            If AvailablegamePart.ContainsKey(sGameType) AndAlso AvailablegamePart(sGameType)(3) Then
                chkQuarter1.Visible = True
                lblQuarter1.Visible = True

                liQuarter1.Visible = True

                game.Visible = True

            End If

        End If


    End Sub


#End Region
    Protected Sub lblGameType_Click(ByVal sender As Object, ByVal e As System.EventArgs)
        If Not (BetType.Equals(_Straight, StringComparison.CurrentCultureIgnoreCase) OrElse LCase(BetType).Contains("if ")) Then
            Return
        End If
        If UserSession.UserType = SBCBL.EUserType.Agent Then
            SelectedPlayerID = ddlPlayers.SelectedValue
        End If
        UserSession.SelectedBetType(Me.SelectedPlayerID) = "SINGLE"

        Session("BetTypeActive") = BetType
        If Not String.IsNullOrEmpty(BetActionLink) AndAlso Not String.IsNullOrEmpty(Me.SelectedPlayerID) Then
            ''clear all game
            UserSession.SelectedGameTypes(Me.SelectedPlayerID).Clear()
            ''get game checked
            GameSelect.Clear()

            Dim olstGameType As List(Of String) = UserSession.SelectedGameTypes(Me.SelectedPlayerID)
            If olstGameType Is Nothing OrElse olstGameType.Count = 0 Then
                ClientAlert("Please Select Game")
            Else
                Response.Redirect(BetActionLink & String.Format("?PlayerID={0}", Me.SelectedPlayerID))
            End If

        Else
            ClientAlert("Please Select A Player")
        End If
    End Sub

    Private Sub GetTeaser(ByVal psTeaserSelect As String)
        Dim oCache As New CCacheManager()
        For Each oTeaserRule As CTeaserRule In oCache.GetTeaserRules()

            If psTeaserSelect.Equals(oTeaserRule.BasketballPoint & "/" & oTeaserRule.FootbalPoint) Then
                Session("TeaserValue") = oTeaserRule
                Return
            End If
        Next
    End Sub

    Protected Sub Teaser_Click(ByVal sender As Object, ByVal e As System.EventArgs)

        If UserSession.UserType = SBCBL.EUserType.Agent Then
            SelectedPlayerID = ddlPlayers.SelectedValue
        End If
        GetTeaser(CType(sender, LinkButton).CommandArgument)
        Session("BetTypeActive") = BetType
        If Not String.IsNullOrEmpty(BetActionLink) AndAlso Not String.IsNullOrEmpty(Me.SelectedPlayerID) Then
            ''clear all game
            UserSession.SelectedGameTypes(Me.SelectedPlayerID).Clear()
            ''get game checked
            GetSelectTeaserGame(rptGameType)
            Dim olstGameType As List(Of String) = UserSession.SelectedGameTypes(Me.SelectedPlayerID)
            If olstGameType Is Nothing OrElse olstGameType.Count = 0 Then
                ClientAlert("Please Select Game1")
            Else
                Response.Redirect(BetActionLink & String.Format("?PlayerID={0}", Me.SelectedPlayerID))
            End If

        Else
            ClientAlert("Please Select A Player")
        End If

    End Sub

    Protected Sub btnContinue_Click(ByVal sender As Object, ByVal e As System.EventArgs)
        If UserSession.UserType = SBCBL.EUserType.Agent Then
            SelectedPlayerID = ddlPlayers.SelectedValue
        End If
        UserSession.SelectedBetType(Me.SelectedPlayerID) = "SINGLE"
        Session("BetTypeActive") = BetType
        If Not String.IsNullOrEmpty(BetActionLink) AndAlso Not String.IsNullOrEmpty(Me.SelectedPlayerID) Then
            ''clear all game
            UserSession.SelectedGameTypes(Me.SelectedPlayerID).Clear()
            ''get game checked
            _log.Error("goi")
            SelectBetType()

            Dim olt As List(Of String) = UserSession.SelectedGameTypes(Me.SelectedPlayerID)
            '_log.Debug("xxx" + olt.Count.ToString())
            Dim olstGameType As List(Of String) = UserSession.SelectedGameTypes(Me.SelectedPlayerID)
            '_log.Debug("sasdasdsa" + olstGameType.Count)

            If olstGameType Is Nothing OrElse olstGameType.Count = 0 Then
                ClientAlert("Please Select Game2")
            Else
                Response.Redirect(BetActionLink & String.Format("?PlayerID={0}", Me.SelectedPlayerID))
            End If

        Else
            ClientAlert("Please Select A Player")
        End If

    End Sub



    Protected Sub lbtPropType_Click(ByVal sender As Object, ByVal e As System.EventArgs)
        Session("BetTypeProp") = True
        UserSession.SelectedGameTypes(SelectedPlayerID).Clear()
        SetSelectedGame("Prop_" + sender.Text, True)
        UserSession.SelectedBetType(Me.SelectedPlayerID) = IIf(True, "PROP", "SINGLE").ToString()
        Session("BetTypeActive") = _BetTheBoard
        Response.Redirect(BetActionLink & String.Format("?PlayerID={0}", Me.SelectedPlayerID))
    End Sub


    Public Sub GetSelectGame(ByVal rptGameType As Repeater)

        Dim sGameType As String
        For j As Integer = 0 To rptGameType.Items.Count - 1
            _log.Debug(j)
            Dim rptSubGameType As Repeater = CType(rptGameType.Items(j).FindControl("rptSubGameType"), Repeater)
            For k As Integer = 0 To rptSubGameType.Items.Count - 1
                'Dim chkGameType As CheckBox = CType(rptSubGameType.Items(k).FindControl("chkGameType"), CheckBox)
                Dim chkOneH, chkTwoH, chkCurrent, chkQuarter1, chkQuarter2, chkQuarter3, chkQuarter4 As CheckBox
                chkOneH = CType(rptSubGameType.Items(k).FindControl("chkOneH"), CheckBox)
                chkTwoH = CType(rptSubGameType.Items(k).FindControl("chkTwoH"), CheckBox)
                chkCurrent = CType(rptSubGameType.Items(k).FindControl("chkCurrent"), CheckBox)
                chkQuarter1 = CType(rptSubGameType.Items(k).FindControl("chkQuarter1"), CheckBox)

                Dim lblGameType As Label = CType(rptSubGameType.Items(k).FindControl("lblGameType"), Label)
                Dim btnGameType = CType(rptSubGameType.Items(k).FindControl("btnGameType"), Button)

                Dim oGame As New CGame()
                If (chkOneH.Checked) Then
                    oGame.OneH = True
                End If
                If (chkTwoH.Checked) Then
                    oGame.TwoH = True
                End If
                If chkCurrent.Checked Then
                    oGame.Current = True
                End If
                If chkQuarter1.Checked Then
                    oGame.Quarter1 = True
                    oGame.Quarter2 = True
                    oGame.Quarter3 = True
                    oGame.Quarter4 = True
                End If


                sGameType = btnGameType.Text
                oGame.GameType = sGameType
                If GameSelect.ContainsKey(sGameType) Then
                    GameSelect.Remove(sGameType)
                End If
                GameSelect.Add(sGameType, oGame)
                '_log.Debug(sGameType + "--" + SafeString(chkCurrent.Checked))
                If chkOneH.Checked OrElse chkTwoH.Checked OrElse chkCurrent.Checked OrElse chkQuarter1.Checked Then
                    SetSelectedGame(sGameType, True)
                Else
                    SetSelectedGame(sGameType, False)
                End If
                Dim olt As List(Of String) = UserSession.SelectedGameTypes(Me.SelectedPlayerID)
                _log.Debug(sGameType & olt.Count.ToString())
            Next
        Next
        'Next

    End Sub

    Public Sub GetSelectTeaserGame(ByVal rptGameType As Repeater)
        For i As Integer = 0 To rptGameType.Items.Count - 1

            Dim sGameType As String
            For j As Integer = 0 To rptGameType.Items.Count - 1
                Dim rptSubGameType As Repeater = CType(rptGameType.Items(j).FindControl("rptSubGameType"), Repeater)
                For k As Integer = 0 To rptSubGameType.Items.Count - 1

                    Dim chkOneH, chkTwoH, chkCurrent, chkQuarter As CheckBox
                    chkOneH = CType(rptSubGameType.Items(k).FindControl("chkOneH"), CheckBox)
                    chkTwoH = CType(rptSubGameType.Items(k).FindControl("chkTwoH"), CheckBox)
                    chkCurrent = CType(rptSubGameType.Items(k).FindControl("chkCurrent"), CheckBox)
                    chkQuarter = CType(rptSubGameType.Items(k).FindControl("chkQuarter"), CheckBox)
                    Dim btnGameType = CType(rptSubGameType.Items(k).FindControl("btnGameType"), Button)
                    Dim lblGameType As Label = CType(rptSubGameType.Items(k).FindControl("lblGameType"), Label)
                    'If chkGameType.Checked Then
                    sGameType = btnGameType.Text
                    _log.Debug(sGameType)
                    '' save check 1h 2h current to session after sent to Betaction
                    If (AvailablegamePart.ContainsKey(sGameType) AndAlso AvailablegamePart(sGameType)(0)) AndAlso (IsFootball(sGameType) OrElse IsBasketball(sGameType)) Then
                        Dim oGame As New CGame()
                        oGame.Current = True
                        oGame.GameType = sGameType
                        If GameSelect.ContainsKey(sGameType) Then
                            GameSelect.Remove(sGameType)
                            SetSelectedGame(sGameType, False)
                        End If
                        GameSelect.Add(sGameType, oGame)
                        SetSelectedGame(sGameType, True)
                    End If
                Next
            Next
        Next
    End Sub



    Protected Sub SelectBetType()


        GetSelectGame(rptGameType)

    End Sub

    Private Sub SetSelectedGame(ByVal psType As String, ByVal pbSelected As Boolean)
        If String.IsNullOrEmpty(Me.SelectedPlayerID) Then
            Return
        End If
        If psType <> "" Then
            If Not pbSelected Then
                UserSession.RemoveSelectedGameType(psType, Me.SelectedPlayerID)
            Else
                UserSession.AddSelectedGameType(psType, Me.SelectedPlayerID)
                Dim olt As List(Of String) = UserSession.SelectedGameTypes(Me.SelectedPlayerID)
                _log.Debug("aaaa" + olt.Count.ToString())
            End If

        End If
    End Sub



    Public Function showImgIcon(ByVal psSportType As String, ByVal psGameType As String) As String
        If psSportType.Equals("Soccer", StringComparison.CurrentCultureIgnoreCase) Then
            Return "<img src='/Images/Soccer/Portugal.png'/>"
        Else
            Return ""
        End If
    End Function

#Region "Available game type"

    Private Function GetAvailableGameType(ByVal plstGameType As List(Of String)) As DataTable
        Dim dicBookMaker As New Dictionary(Of String, String)
        Dim oToday = UserSession.ConvertToEST(DateTime.Now.ToUniversalTime()).AddDays(-1)
        For Each sGameType As String In plstGameType
            ' ORIGINAL CODE
            'Dim sBookmaker As String = UserSession.SysSettings(GetAgentBookMaker(sGameType)).GetValue(sGameType)
            'If (sGameType.Contains("Football") OrElse sGameType.Contains("Basketball")) AndAlso (UserSession.Cache.GetSysSettings(SuperAgentId & "FixexdSpreadMoney").Count > 0 AndAlso UserSession.Cache.GetSysSettings(SuperAgentId & "FixexdSpreadMoney").GetBooleanValue("IsFlatSpreadMoney")) Then
            '    dicBookMaker(sGameType) = SuperAgentId & " Manipulation"
            'Else
            '    If String.IsNullOrEmpty(sBookmaker) Then
            '        sBookmaker = "Pinnacle2"
            '    End If
            '    dicBookMaker(sGameType) = sBookmaker
            'End If

            ' NEW CODE
            Dim sBookmaker As String = UserSession.SysSettings(GetAgentBookMaker(sGameType)).GetValue(sGameType)
            If String.IsNullOrEmpty(sBookmaker) Then
                sBookmaker = "Pinnacle2"
            End If
            dicBookMaker(sGameType) = sBookmaker
        Next
        Dim odtGames = New SBCBL.Managers.CGameManager().GetExistsAvailableGames(dicBookMaker, oToday.AddDays(-1), True, True, False, "", SuperAgentId, True)
        Return odtGames
    End Function


#End Region


End Class



