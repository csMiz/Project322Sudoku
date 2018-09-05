Imports System.IO
Imports System.Text.RegularExpressions

Public Class SudokuMap
    ''' <summary>
    ''' 从左上角开始 Block(第几列 X,第几行 Y)
    ''' </summary>
    Public Block(8, 8) As SudokuBlock

    Private SolvingStack As New Stack(Of SudokuBlock)

    Public Sub New()
        For i = 0 To 8
            For j = 0 To 8
                Block(i, j) = New SudokuBlock
            Next
        Next
    End Sub

    Public Async Function SolveSudoku() As Task(Of Boolean)
label3:
        Dim result As SolveResult = Await FindCandidates()
        If result = SolveResult.Complete Then Return True
        If result = SolveResult.HasError Then
label4:
            If SolvingStack.Count = 0 Then Throw New Exception("stack is empty!")
            Dim lastGuess As SudokuBlock = SolvingStack.Pop
            For i = 0 To 8
                For j = 0 To 8
                    If Block(i, j).Status = SudokuBlockStatus.TryingDecided AndAlso Block(i, j).TryingBindingBlock.Equals(lastGuess) Then
                        Block(i, j).Status = SudokuBlockStatus.NotDecided
                        Block(i, j).TryingBindingBlock = Nothing
                    End If
                Next
            Next
            If lastGuess.TryNext Then
                SolvingStack.Push(lastGuess)
                GoTo label3
            Else
                lastGuess.Status = SudokuBlockStatus.NotDecided
                lastGuess.ClearTry()
                GoTo label4
            End If

        ElseIf result = SolveResult.NoChangeNotComplete Then
            Dim guessNext As SudokuBlock
            For i = 0 To 8
                For j = 0 To 8
                    If Block(i, j).Status = SudokuBlockStatus.NotDecided Then
                        guessNext = Block(i, j)
                        GoTo label2
                    End If
                Next
            Next
            Throw New Exception("already completed!")
label2:
            guessNext.Status = SudokuBlockStatus.Trying
            SolvingStack.Push(guessNext)
            GoTo label3
        Else
            Throw New Exception("solving error!")
        End If


    End Function

    ''' <summary>
    ''' 填充可能值
    ''' </summary>
    Private Async Function FindCandidates() As Task(Of SolveResult)
        For i = 0 To 8
            For j = 0 To 8
                If Block(i, j).Status = SudokuBlockStatus.None OrElse Block(i, j).Status = SudokuBlockStatus.NotDecided Then
                    With Block(i, j)
                        .Value.Clear()
                        For k = 1 To 9
                            .Value.Add(k)
                        Next
                        .Status = SudokuBlockStatus.NotDecided
                    End With
                End If
            Next
        Next

        Dim result As SolveResult

label1:

        FilterRows()
        FilterColumns()
        FilterNineBlocks()
        result = CheckBlocks()

        Call Form1.PaintSudoku()
        Application.DoEvents()
        Await Task.Delay(2000)

        If result = SolveResult.ChangedNotComplete Then GoTo label1

        Return result
    End Function

    ''' <summary>
    ''' 分析行
    ''' </summary>
    Private Sub FilterRows()
        For j = 0 To 8
            For i = 0 To 8
                If Block(i, j).Status = SudokuBlockStatus.Fixed OrElse Block(i, j).Status = SudokuBlockStatus.Trying OrElse Block(i, j).Status = SudokuBlockStatus.Decided OrElse Block(i, j).Status = SudokuBlockStatus.TryingDecided Then
                    Dim excludeValue As Byte = Block(i, j).GetValue
                    For i2 = 0 To 8
                        If Block(i2, j).Status = SudokuBlockStatus.NotDecided Then
                            If Block(i2, j).Value.Contains(excludeValue) Then Block(i2, j).Value.Remove(excludeValue)
                        End If
                    Next
                End If
            Next
        Next
    End Sub

    ''' <summary>
    ''' 分析列
    ''' </summary>
    Private Sub FilterColumns()
        For i = 0 To 8
            For j = 0 To 8
                If Block(i, j).Status = SudokuBlockStatus.Fixed OrElse Block(i, j).Status = SudokuBlockStatus.Trying OrElse Block(i, j).Status = SudokuBlockStatus.Decided OrElse Block(i, j).Status = SudokuBlockStatus.TryingDecided Then
                    Dim excludeValue As Byte = Block(i, j).GetValue
                    For j2 = 0 To 8
                        If Block(i, j2).Status = SudokuBlockStatus.NotDecided Then
                            If Block(i, j2).Value.Contains(excludeValue) Then Block(i, j2).Value.Remove(excludeValue)
                        End If
                    Next
                End If
            Next
        Next
    End Sub

    ''' <summary>
    ''' 分析九宫格
    ''' </summary>
    Private Sub FilterNineBlocks()
        For i = 0 To 8
            For j = 0 To 8
                If Block(i, j).Status = SudokuBlockStatus.Fixed OrElse Block(i, j).Status = SudokuBlockStatus.Trying OrElse Block(i, j).Status = SudokuBlockStatus.Decided OrElse Block(i, j).Status = SudokuBlockStatus.TryingDecided Then
                    Dim excludeValue As Byte = Block(i, j).GetValue
                    For i2 = (i \ 3) * 3 To (i \ 3) * 3 + 2
                        For j2 = (j \ 3) * 3 To (j \ 3) * 3 + 2
                            If Block(i2, j2).Status = SudokuBlockStatus.NotDecided Then
                                If Block(i2, j2).Value.Contains(excludeValue) Then Block(i2, j2).Value.Remove(excludeValue)
                            End If
                        Next
                    Next
                End If
            Next
        Next
    End Sub

    Public Function CheckBlocks() As SolveResult
        Dim changed As Boolean = False
        Dim completed As Boolean = True
        For i = 0 To 8
            For j = 0 To 8
                If Block(i, j).Status = SudokuBlockStatus.NotDecided Then
                    completed = False
                    If Block(i, j).Value.Count = 0 Then
                        Return SolveResult.HasError
                    ElseIf Block(i, j).Value.Count = 1 Then
                        If SolvingStack.Count = 0 Then
                            Block(i, j).Status = SudokuBlockStatus.Decided
                        Else
                            Block(i, j).Status = SudokuBlockStatus.TryingDecided
                            Block(i, j).TryingBindingBlock = SolvingStack(0)
                        End If
                        changed = True
                    End If
                End If
            Next
        Next
        For i = 0 To 8
            Dim col As New List(Of Byte)
            For j = 0 To 8
                If Block(i, j).Status = SudokuBlockStatus.Fixed OrElse Block(i, j).Status = SudokuBlockStatus.Trying OrElse Block(i, j).Status = SudokuBlockStatus.Decided OrElse Block(i, j).Status = SudokuBlockStatus.TryingDecided Then
                    Dim value As Byte = Block(i, j).GetValue
                    If col.Contains(value) Then
                        Return SolveResult.HasError
                    Else
                        col.Add(value)
                    End If
                End If
            Next
        Next
        For j = 0 To 8
            Dim row As New List(Of Byte)
            For i = 0 To 8
                If Block(i, j).Status = SudokuBlockStatus.Fixed OrElse Block(i, j).Status = SudokuBlockStatus.Trying OrElse Block(i, j).Status = SudokuBlockStatus.Decided OrElse Block(i, j).Status = SudokuBlockStatus.TryingDecided Then
                    Dim value As Byte = Block(i, j).GetValue
                    If row.Contains(value) Then
                        Return SolveResult.HasError
                    Else
                        row.Add(value)
                    End If
                End If
            Next
        Next
        For i = 0 To 2
            For j = 0 To 2
                Dim nineblocks As New List(Of Byte)
                For i2 = i * 3 To i * 3 + 2
                    For j2 = j * 3 To j * 3 + 2
                        If Block(i2, j2).Status = SudokuBlockStatus.Fixed OrElse Block(i2, j2).Status = SudokuBlockStatus.Trying OrElse Block(i2, j2).Status = SudokuBlockStatus.Decided OrElse Block(i2, j2).Status = SudokuBlockStatus.TryingDecided Then
                            Dim value As Byte = Block(i2, j2).GetValue
                            If nineblocks.Contains(value) Then
                                Return SolveResult.HasError
                            Else
                                nineblocks.Add(value)
                            End If
                        End If
                    Next
                Next
            Next
        Next

        If completed Then Return SolveResult.Complete
        If changed Then Return SolveResult.ChangedNotComplete
        Return SolveResult.NoChangeNotComplete
    End Function

    Public Sub LoadFromFile()
        Dim stream As FileStream = New FileStream(Application.StartupPath & "\sudoku.txt", FileMode.Open)
        Dim content As String
        Using sr As StreamReader = New StreamReader(stream)
            content = sr.ReadToEnd
        End Using
        stream.Close()
        stream.Dispose()

        content = content.Replace(vbCrLf, vbNullString)

        Dim blockValue() As String = Regex.Split(content, ",")
        For i = 0 To 80
            With Block(i Mod 9, i \ 9)
                If blockValue(i) = "" OrElse blockValue(i) = "0" Then
                    .Status = SudokuBlockStatus.None
                Else
                    Dim value As Byte = CInt(blockValue(i))
                    .Value.Add(value)
                    .Status = SudokuBlockStatus.Fixed
                End If
            End With
        Next

    End Sub

    Public Sub PaintMap(G As Graphics)
        For i = 0 To 9
            G.DrawLine(Pen1, i * 100, 0, i * 100, 900)
            G.DrawLine(Pen1, 0, i * 100, 900, i * 100)
        Next
        For i = 0 To 3
            G.DrawLine(Pen2, i * 300, 0, i * 300, 900)
            G.DrawLine(Pen2, 0, i * 300, 900, i * 300)
        Next
        For i = 0 To 8
            For j = 0 To 8
                If Block(i, j).Status = SudokuBlockStatus.Fixed Then
                    G.DrawString(Block(i, j).GetValue.ToString, font1, brush1, i * 100 + 10, j * 100 + 10)
                ElseIf Block(i, j).Status = SudokuBlockStatus.Decided Then
                    G.DrawString(Block(i, j).GetValue.ToString, font1, brush2, i * 100 + 10, j * 100 + 10)
                ElseIf Block(i, j).Status = SudokuBlockStatus.Trying Then
                    G.DrawString(Block(i, j).GetValue.ToString, font1, brush3, i * 100 + 10, j * 100 + 10)
                ElseIf Block(i, j).Status = SudokuBlockStatus.TryingDecided Then
                    G.DrawString(Block(i, j).GetValue.ToString, font1, brush4, i * 100 + 10, j * 100 + 10)
                End If
            Next
        Next

    End Sub

End Class

Public Enum SolveResult As Byte
    NoChangeNotComplete = 0
    ChangedNotComplete = 1
    Complete = 2
    HasError = 3
End Enum
