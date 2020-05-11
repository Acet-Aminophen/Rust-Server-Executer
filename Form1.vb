Imports System.Net.Sockets
Imports System.Text
Imports System.IO
Imports System.IO.FileStream
Public Class Form1
    Dim test As Boolean = False

    Dim port As String = ""
    Dim rconport As String = ""
    Dim title As String = ""

    Dim worldsize As String = "2600"
    Dim seed As String = "15"
    Dim tier2() As String = {
    "ammo.pistol.fire",
    "ammo.pistol.hv",
    "ammo.rifle",
    "ammo.rocket.fire",
    "ammo.rocket.hv",
    "ammo.shotgun",
    "ammo.shotgun.fire",
    "ammo.shotgun.slug",
    "floor.ladder.hatch",
    "wall.external.high.stone",
    "wall.frame.cell.gate",
    "wall.frame.cell",
    "roadsign.gloves",
    "coffeecan.helmet",
    "heavy.plate.helmet",
    "hoodie",
    "heavy.plate.jacket",
    "heavy.plate.pants",
    "roadsign.kilt",
    "shoes.boots",
    "hazmatsuit",
    "roadsign.jacket",
    "barricade.concrete",
    "barricade.metal",
    "furnace.large",
    "trap.landmine",
    "locker",
    "small.oil.refinery",
    "electric.battery.rechargable.large",
    "electric.battery.rechargable.medium",
    "electric.counter",
    "electric.hbhfsensor",
    "electric.andswitch",
    "electric.rf.broadcaster",
    "electric.rf.receiver",
    "electric.fuelgenerator.small",
    "searchlight",
    "water.catcher.large",
    "generator.wind.scrap",
    "autoturret",
    "ammo.rocket.sam",
    "largemedkit",
    "syringe.medical",
    "rf_pager",
    "weapon.mod.muzzleboost",
    "weapon.mod.muzzlebrake",
    "chainsaw",
    "grenade.f1",
    "flamethrower",
    "pistol.python",
    "rocket.launcher",
    "axe.salvaged",
    "icepick.salvaged",
    "shotgun.pump",
    "pistol.semiauto",
    "rifle.semiauto",
    "smg.2",
    "longsword",
    "smg.thompson"
    }
    Dim tier2_many() As Integer = {
    30,
    30,
    30,
    2,
    2,
    10,
    10,
    10,
    1,
    1,
    1,
    1,
    2,
    2,
    1,
    2,
    1,
    1,
    2,
    2,
    3,
    2,
    5,
    3,
    1,
    3,
    1,
    1,
    1,
    1,
    1,
    1,
    1,
    1,
    1,
    1,
    1,
    1,
    1,
    1,
    10,
    3,
    5,
    1,
    1,
    1,
    1,
    3,
    1,
    1,
    1,
    3,
    3,
    1,
    1,
    1,
    1,
    3,
    1
    }
    Dim tier3() As String = {
    "door.hinged.toptier",
    "door.double.hinged.toptier",
    "gates.external.high.stone",
    "wall.window.bars.toptier",
    "explosives",
    "metal.plate.torso",
    "metal.facemask",
    "explosive.timed",
    "weapon.mod.8x.scope",
    "rifle.ak",
    "rifle.bolt",
    "weapon.mod.holosight",
    "smg.mp5",
    "weapon.mod.lasersight",
    "ammo.rifle.explosive",
    "ammo.rifle.hv",
    "ammo.rifle.incendiary",
    "ammo.rocket.basic"
    }
    Dim tier3_many() As Integer = {
    1,
    1,
    1,
    1,
    5,
    1,
    1,
    1,
    1,
    1,
    1,
    1,
    1,
    1,
    30,
    30,
    30,
    1
    }

    Dim server_on As Boolean = False
    Dim server_active As Boolean = False
    Dim server_ontime As Integer = 0
    Dim process_id As Integer
    Dim give_tier As Integer = 0
    Dim GiveItemProcess As Boolean = True
    Dim chk_key As String
    Dim players As Integer = 0
    Dim run_time As Integer = 0

    Dim time_serverstart As String = ""
    Dim time_now_year As String
    Dim time_now_month As String
    Dim time_now_day As String
    Dim time_now_hour As String
    Dim time_now_min As String
    Dim time_now_sec As String

    Dim commands As New List(Of String)

    Dim clientlist As New List(Of TcpClient)
    Dim serversocket As New TcpListener(8888)

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        set_time()
        ServerStart()

        serversocket.Start()
        TCP_Log_Adder("LISTER IS ONLINE" + vbCrLf)
        TCP_Timer.Enabled = True
    End Sub

    Private Sub TCP_Timer_Tick(sender As Object, e As EventArgs) Handles TCP_Timer.Tick
        While serversocket.Pending()
            Dim clientsocket As TcpClient = serversocket.AcceptTcpClient()
            Dim getclientinfo As Net.IPEndPoint = clientsocket.Client.RemoteEndPoint
            Dim clinetip_port As String = getclientinfo.Address.ToString + ":" + getclientinfo.Port.ToString
            TCP_Log_Adder(Convert.ToString(clinetip_port) + "가 접속" + vbCrLf)
            clientlist.Add(clientsocket)
        End While

        For index = 0 To clientlist.Count - 1
            Dim bytesfrom(1024) As Byte
            Dim buffsize As Integer
            Dim datafromclient As String
            Dim sendbytes As [Byte]()
            Dim serverresponse As String
            Dim clientsocket = clientlist(index)
            Dim networkstream As NetworkStream = clientsocket.GetStream

            Try
                serverresponse = "Connected Check$"
                sendbytes = Encoding.Default.GetBytes(serverresponse)
                networkstream.Write(sendbytes, 0, sendbytes.Length)
            Catch ex As Exception
                clientlist(index).Close()
                clientlist.RemoveAt(index)
                TCP_Log_Adder("클라이언트" + index.ToString + "에서 연결 해제" + vbCrLf)
                Return
            End Try

            If networkstream.DataAvailable = True Then
                clientsocket.ReceiveBufferSize = 1024
                buffsize = clientsocket.ReceiveBufferSize
                networkstream.Read(bytesfrom, 0, buffsize)
                datafromclient = System.Text.Encoding.Default.GetString(bytesfrom)
                datafromclient = datafromclient.Substring(0, datafromclient.IndexOf("$"))
                TCP_Log_Adder("IN : " + datafromclient + vbCrLf)
                serverresponse = """" + datafromclient + """의 송신을 확인함$"
                sendbytes = Encoding.Default.GetBytes(serverresponse)
                networkstream.Write(sendbytes, 0, sendbytes.Length)
                Add_Command(datafromclient + "{ENTER}")
            End If
        Next
    End Sub

    Sub msg(ByVal mesg As String)
        mesg.Trim()
        TCP_Log_Adder(" >> " + mesg + vbCrLf)
    End Sub
    Private Sub Display_Timer_Tick(sender As Object, e As EventArgs) Handles Display_Timer.Tick

        If server_on = True Then
            server_ontime += 1
        End If

        Dim temp_ontime_min As String = Int(server_ontime / 60).ToString
        Dim temp_ontime_sec As String = Int(server_ontime Mod 60).ToString
        Dim temp_runtime_min As String = Int(run_time / 60).ToString
        Dim temp_runtime_sec As String = Int(run_time Mod 60).ToString
        set_time()

        Label1.Text = "TIME : " + time_now_min + "분 " + time_now_sec + "초"
        Label2.Text = "ACTT : " + temp_ontime_min + "분 " + temp_ontime_sec + "초 가동중"
        Label3.Text = "ACTT : " + temp_runtime_min + "분 " + temp_runtime_sec + "초 가동중"

        If GiveItemProcess = True Then
            If (Now.Minute Mod 10 = 9) Then
                If (Now.Minute = 29) Or (Now.Minute = 59) Then
                    give_tier = 3
                Else
                    give_tier = 2
                End If
                Notice("TIER" + give_tier.ToString + " ITEM ETA 1MIN")
                GiveItemProcess = False
                AMin.Enabled = True
            End If
        End If

        Dim str As String =
"Display_Timer : " + Display_Timer.Enabled.ToString + vbCrLf +
"AMin : " + AMin.Enabled.ToString + vbCrLf +
"CHK_Players : " + CHK_Players.Enabled.ToString + vbCrLf +
"ASec : " + ASec.Enabled.ToString + vbCrLf +
"CHK_Restart : " + CHK_Restart.Enabled.ToString + vbCrLf +
"restart_timer : " + restart_timer.Enabled.ToString + vbCrLf +
"before_server_active : " + before_server_active.Enabled.ToString + vbCrLf +
"CHK_SERVER_ACTIVE : " + CHK_SERVER_ACTIVE.Enabled.ToString + vbCrLf + vbCrLf +
"players : " + players.ToString + vbCrLf +
"server_active : " + server_active.ToString + vbCrLf +
"server_ontime : " + server_ontime.ToString + vbCrLf +
"server_on : " + server_on.ToString + vbCrLf +
"GiveItemProcess : " + GiveItemProcess.ToString + vbCrLf +
"time_serverstart : " + time_serverstart + vbCrLf

        TextBox3.Text = ""
        TextBox3.AppendText(str)
    End Sub
    Private Sub Write_Custom_Log(target As String)
        My.Computer.FileSystem.WriteAllText("C:\rustserver\SVL_CUSTOM\log.txt", target, True)
    End Sub
    Private Sub AMin_Tick(sender As Object, e As EventArgs) Handles AMin.Tick
        Giveitem(give_tier)
        Giveitem(0, "scrap", 200)
        Notice("ITEMS ARE ARRIVED")
        GiveItemProcess = True
        AMin.Enabled = False
    End Sub
    Private Function Giveitem(type As Integer, Optional target As String = "", Optional many As Integer = 1) As Integer
        If server_active = False Then
            Return 0
        End If

        Randomize()
        Dim str As String = ""
        If type = 0 Then
            str = "inventory.giveall " + target + " " + many.ToString
        ElseIf type = 2 Then
            Dim index_item As Integer = Int(Rnd() * tier2.Count)
            str = "inventory.giveall " + tier2(index_item) + " " + tier2_many(index_item).ToString
        ElseIf type = 3 Then
            Dim index_item As Integer = Int(Rnd() * tier3.Count)
            str = "inventory.giveall " + tier3(index_item) + " " + tier3_many(index_item).ToString
        End If
        Add_Command(str + "{ENTER}")
        give_tier = 0

        addlog("아이템 출력 + " + type.ToString)
        Return 0
    End Function
    Private Function Notice(target As String) As Integer
        If (server_active = False) Then
            Return 0
        End If

        Add_Command("say | " + target + "{ENTER}")

        addlog("서버 공지 = " + target)
        Return 0
    End Function
    Private Sub CHK_Players_Tick(sender As Object, e As EventArgs) Handles CHK_Players.Tick
        If (server_active = False) Then
            Return
        End If

        chk_key = "CHKKEY"
        Randomize()
        For index = 1 To 10
            chk_key += Int(Rnd() * 10).ToString()
        Next

        Add_Command(chk_key + "{ENTER}")
        Add_Command("players" + "{ENTER}")
        ASec.Enabled = True
    End Sub
    Private Sub ASec_Tick(sender As Object, e As EventArgs) Handles ASec.Tick
        ASec.Enabled = False
        Dim log_reader As FileStream = New FileStream("C:\rustserver\SVL\" + time_serverstart + "SVL.txt", FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
        Dim log_reader_str As StreamReader = New StreamReader(log_reader)
        Dim saver As StreamReader = log_reader_str
        Dim log As String = saver.ReadToEnd
        My.Computer.FileSystem.WriteAllText("C:\rustserver\SVL_CUSTOM\now.txt", log, False)
        While (True)
            log = log_reader_str.ReadLine
            Try
                If log.Equals("Command '" + chk_key + "' not found") Then
                    Exit While
                End If
            Catch ex As Exception
                addlog("인원 사항 수정 에러")
                Return
            End Try
        End While
        While (True)
            log = log_reader_str.ReadLine
            If ((0 <> InStr(log, "id")) And (0 <> InStr(log, "name")) And (0 <> InStr(log, "ping"))) Then
                Exit While
            End If
        End While
        players = 0
        While (True)
            log = log_reader_str.ReadLine
            log = log_reader_str.ReadLine
            If log.Equals("") Then
                Exit While
            Else
                players += 1
            End If
        End While
        Label2.Text = players.ToString

        addlog("인원 사항 수정 = " + players.ToString)
    End Sub
    Private Function ServerStart() As Integer
        CHK_Players.Enabled = True
        server_on = True
        server_ontime = 0

        time_serverstart = time_now_year + time_now_month + time_now_day + time_now_hour + time_now_min + time_now_sec

        If test = True Then
            port = "28018"
            rconport = "28019"
            title = "1313 | test server"
        Else
            port = "28015"
            rconport = "28016"
            title = "[KR]청렴|아이템 랜덤 박스|초기화 2월 3일"
        End If

        Dim in_file As String = "CENSORED"
fps.limit ""256""
global.perf ""0""
server.arrowarmor ""1""
server.arrowdamage ""1""
server.bleedingarmor ""1""
server.bleedingdamage ""1""
server.bulletarmor ""1""
server.bulletdamage ""1""
server.meleearmor ""1""
server.meleedamage ""1""
server.playerserverfall ""True""
server.showholstereditems ""True""
server.woundingenabled ""True""
server.idlekick 20
server.idlekickmode 2
server.idlekickadmins 0"
        My.Computer.FileSystem.WriteAllText("C:\rustserver\RustServer.bat", in_file, False)
        My.Computer.FileSystem.WriteAllText("C:\rustserver\server\FIRST\cfg\serverauto.cfg", cfg_file, False)
        process_id = Shell("C:\rustserver\RustServer.bat")
        before_server_active.Enabled = True
        CHK_Restart.Enabled = True

        addlog("서버 시작")

        Return 0
    End Function
    Private Function ServerQuit() As Integer
        server_on = False
        server_active = False
        server_ontime = 0
        CHK_Players.Enabled = False
        CHK_Restart.Enabled = False
        Commander_Timer.Enabled = False
        Add_Command("server.save" + "{ENTER}")
        Add_Command("server.writecfg" + "{ENTER}")
        Add_Command("quit" + "{ENTER}")
        addlog("서버 종료")

        Return 0
    End Function
    Private Function Restart() As Integer
        addlog("서버 재시작 시퀸스 시작")

        ServerQuit()
        restart_timer.Enabled = True
        Return 0
    End Function
    Private Sub restart_timer_Tick(sender As Object, e As EventArgs) Handles restart_timer.Tick
        ServerStart()
        restart_timer.Enabled = False
    End Sub
    Private Sub CHK_Restart_Tick(sender As Object, e As EventArgs) Handles CHK_Restart.Tick
        If (players = 0) And (server_ontime > 10800) And (server_active = True) Then
            Restart()
        End If
    End Sub
    Private Sub CHK_SERVER_ACTIVE_Tick(sender As Object, e As EventArgs) Handles CHK_SERVER_ACTIVE.Tick
        Dim log_reader As FileStream = New FileStream("C:\rustserver\SVL\" + time_serverstart + "SVL.txt", FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
        Dim log_reader_str As StreamReader = New StreamReader(log_reader)
        Dim log As String = log_reader_str.ReadToEnd
        If (0 <> InStr(log, "SteamServer Connected")) Then
            server_active = True
            CHK_SERVER_ACTIVE.Enabled = False
            Commander_Timer.Enabled = True

            addlog("서버 상태 ACTIVE로 확인")
        End If
    End Sub
    Private Sub before_server_active_Tick(sender As Object, e As EventArgs) Handles before_server_active.Tick
        CHK_SERVER_ACTIVE.Enabled = True
        before_server_active.Enabled = False
    End Sub
    Private Function addlog(target As String) As Integer
        Dim str As String = time_now_year + time_now_month + time_now_day + time_now_hour + time_now_min + time_now_sec + " | " + target + vbCrLf
        TextBox1.AppendText(str)
        Write_Custom_Log(str)
        Return 0
    End Function
    Private Function set_time() As Integer
        Dim temp_now_year As Integer = Now.Year
        Dim temp_now_month As Integer = Now.Month
        Dim temp_now_day As Integer = Now.Day
        Dim temp_now_hour As Integer = Now.Hour
        Dim temp_now_min As Integer = Now.Minute
        Dim temp_now_sec As Integer = Now.Second

        time_now_year = temp_now_year.ToString
        If (temp_now_month < 10) Then
            time_now_month = "0" + temp_now_month.ToString
        Else
            time_now_month = temp_now_month.ToString
        End If

        If (temp_now_day < 10) Then
            time_now_day = "0" + temp_now_day.ToString
        Else
            time_now_day = temp_now_day.ToString
        End If

        If (temp_now_hour = 0) Then
            time_now_hour = "00"
        ElseIf (temp_now_hour < 10) Then
            time_now_hour = "0" + temp_now_hour.ToString
        Else
            time_now_hour = temp_now_hour.ToString
        End If

        If (temp_now_min = 0) Then
            time_now_min = "00"
        ElseIf (temp_now_min < 10) Then
            time_now_min = "0" + temp_now_min.ToString
        Else
            time_now_min = temp_now_min.ToString
        End If

        If (temp_now_sec = 0) Then
            time_now_sec = "00"
        ElseIf (temp_now_sec < 10) Then
            time_now_sec = "0" + temp_now_sec.ToString
        Else
            time_now_sec = temp_now_sec.ToString
        End If

        Return 0
    End Function
    Private Sub Run_Timer_Tick(sender As Object, e As EventArgs) Handles Run_Timer.Tick
        run_time += 1
    End Sub

    Private Sub Add_Command(target As String)
        commands.Add(target)
    End Sub

    Private Sub Commander_Timer_Tick(sender As Object, e As EventArgs) Handles Commander_Timer.Tick
        Dim many As Integer = commands.Count
        If many = 0 Then
            Return
        End If
        For index = 0 To many - 1
            AppActivate(process_id)
            SendKeys.Send(commands(0))
            commands.RemoveAt(0)
        Next
    End Sub

    Private Sub TCP_Log_Adder(target As String)
        TextBox4.AppendText(target)
    End Sub
End Class
