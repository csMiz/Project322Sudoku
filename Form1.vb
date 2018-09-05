Public Class Form1

    Public map As New SudokuMap

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Show()
        Height = 580
        Width = 750
        map.LoadFromFile()

        Call PaintSudoku()


    End Sub

    Public Sub PaintSudoku()
        P.Image = Nothing
        Dim bm As New Bitmap(900, 900)
        Dim G As Graphics = Graphics.FromImage(bm)
        G.Clear(Color.White)

        map.PaintMap(G)

        P.Image = bm
        P.Refresh()
        G.Dispose()
    End Sub

    Private Async Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Dim result = Await map.SolveSudoku()
        If result Then
            PaintSudoku()
            MsgBox("finished")
        Else
            MsgBox("failed")
        End If
    End Sub
End Class
