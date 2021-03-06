Option Explicit
Option Strict


' Dynamic Bandwidth Monitor
' Leak detection method implemented in a real-time data historian
' Copyright (C) 2014-2018  J.H. Fitié, Vitens N.V.
'
' This file is part of DBM.
'
' DBM is free software: you can redistribute it and/or modify
' it under the terms of the GNU General Public License as published by
' the Free Software Foundation, either version 3 of the License, or
' (at your option) any later version.
'
' DBM is distributed in the hope that it will be useful,
' but WITHOUT ANY WARRANTY; without even the implied warranty of
' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
' GNU General Public License for more details.
'
' You should have received a copy of the GNU General Public License
' along with DBM.  If not, see <http://www.gnu.org/licenses/>.


Imports System
Imports System.Collections.Generic
Imports System.Double
Imports System.IO


' Assembly title
<assembly:System.Reflection.AssemblyTitle("DBMPointDriverCSV")>


Namespace Vitens.DynamicBandwidthMonitor


  Public Class DBMPointDriver
    Inherits DBMPointDriverAbstract


    ' Description: Driver for CSV files (timestamp,value).
    ' Identifier (Point): String (CSV filename)
    ' Remarks: Data interval must be the same as the CalculationInterval
    '          parameter.


    Private Values As New Dictionary(Of DateTime, Double)
    Private Shared SplitChars As Char() = {","c, "	"c} ' Comma and tab


    Public Sub New(Point As Object)

      MyBase.New(Point)

    End Sub


    Public Overrides Sub PrepareData(StartTimestamp As DateTime,
      EndTimestamp As DateTime)

      ' Retrieves information from a CSV file and stores this in the Values
      ' dictionary. Passed timestamps are ignored and all data in the CSV is
      ' loaded into memory.

      Dim Substrings() As String
      Dim Timestamp As DateTime
      Dim Value As Double

      Values.Clear
      If File.Exists(DirectCast(Point, String)) Then
        Using StreamReader As New StreamReader(DirectCast(Point, String))
          Do While Not StreamReader.EndOfStream
            ' Comma and tab delimiters; split timestamp and value
            Substrings = StreamReader.ReadLine.Split(SplitChars, 2)
            If Substrings.Length = 2 Then
              If DateTime.TryParse(Substrings(0), Timestamp) Then
                If Double.TryParse(Substrings(1), Value) Then
                  If Not Values.ContainsKey(Timestamp) Then
                    Values.Add(Timestamp, Value) ' Add data to dictionary
                  End If
                End If
              End If
            End If
          Loop
        End Using ' Close CSV file
      Else
        ' If Point does not represent a valid, existing file then throw a
        ' File Not Found Exception.
        Throw New FileNotFoundException(DirectCast(Point, String))
      End If

    End Sub


    Public Overrides Function GetData(Timestamp As DateTime) As Double

      ' GetData retrieves data from the Values dictionary. Non existing
      ' timestamps return NaN.

      If Values.Count = 0 Then PrepareData(DateTime.MinValue, DateTime.MaxValue)

      ' Look up data from memory
      GetData = Nothing
      If Values.TryGetValue(Timestamp, GetData) Then ' In cache
        Return GetData ' Return value from cache
      Else
        Return NaN ' No data in memory for timestamp, return Not a Number.
      End If

    End Function


  End Class


End Namespace
