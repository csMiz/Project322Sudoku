Public Class SudokuBlock
    Public Property Value As New List(Of Byte)
    Public Status As SudokuBlockStatus

    Private GuessIndex As Byte = 0
    Public TryingBindingBlock As SudokuBlock = Nothing

    Public Function GetValue() As Byte
        If Status = SudokuBlockStatus.Trying Then
            Return Value(GuessIndex)
        Else
            If Value.Count = 0 Then
                Throw New Exception("no value")
            End If
            Return Value(0)
        End If
    End Function

    Public Function TryNext() As Boolean
        GuessIndex += 1
        If GuessIndex >= Value.Count Then
            Return False
        End If
        Return True
    End Function

    Public Sub ClearTry()
        GuessIndex = 0
    End Sub

End Class

Public Enum SudokuBlockStatus As Byte
    None = 0
    Fixed = 1
    Decided = 2
    Trying = 3
    NotDecided = 4
    TryingDecided = 5
End Enum