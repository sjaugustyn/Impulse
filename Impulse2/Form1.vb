Imports System
Imports System.IO
Imports System.Collections
Imports Microsoft.VisualBasic
Imports System.Drawing.Drawing2D
Imports System.Drawing.Imaging
Imports System.Linq
Imports System.Object
Imports System.Threading

Public Class Form1

    'declare global variables
    Dim filename As String

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click

        Dim filelimit = 100000 'limit of number of readings in file
        Dim gaugedatamax(5) As Single 'first value is max, second value is location, third/fourth is max LM, fifth/sixth is max LB

        'clear charts
        Me.Chart1.Series.Clear()
        Me.Chart2.Series.Clear()

        ' fill in default if no file selected
        If TextBox1.Text = "" Then
            filename = "C:\Users\augustyns\Documents\20161114 Boston Logan\bos danymic data\20170202\Taxiway B_Dynamic Measurements_2017-0202-000-747-436.tab"
            TextBox1.Text = filename
        Else
            filename = TextBox1.Text
        End If

        ' read tab file
        Dim MyReaderL As New Microsoft.VisualBasic.
        FileIO.TextFieldParser(filename)

        MyReaderL.TextFieldType = FileIO.FieldType.Delimited
        MyReaderL.SetDelimiters(vbTab&)

        Dim currentRowL As String()
        Dim alldata(filelimit, 100) As Single
        Dim RowL As Integer = 0
        Dim ColumnL As Integer = 0
        Dim LengthL As Integer

        While Not MyReaderL.EndOfData
            currentRowL = MyReaderL.ReadFields()
            Dim currentField As String
            For Each currentField In currentRowL
                If IsNumeric(currentField) Then
                    alldata(RowL, ColumnL) = currentField
                End If
                ColumnL += 1
            Next
            RowL += 1
            ColumnL = 0
        End While
        LengthL = RowL - 1

        'separate time x axis
        Dim timex(LengthL - 7) As Single
        For i = 0 To LengthL - 7
            timex(i) = alldata(i + 7, 0)
        Next
        'MessageBox.Show(timex(LengthL - 7))

        'separate and zero all data
        Dim gauges As String = "LM1-TSG-1	LM1-TSG-2	LM1-LSG-1	LM1-LSG-2	LM1-TSG-3	LM1-TSG-4	LM1-LSG-3	LM1-LSG-4	LM2-TSG-1	LM2-TSG-2	LM2-LSG-1	LM2-LSG-2	LM2-TSG-3	LM2-TSG-4	LM2-LSG-3	LM2-LSG-4	LM3-TSG-1	LM3-TSG-2	LM3-LSG-1	LM3-LSG-2	LM3-TSG-3	LM3-TSG-4	LM3-LSG-3	LM3-LSG-4	LB1-TSG-1	LB1-TSG-2	LB1-LSG-1	LB1-LSG-2	LB1-TSG-3	LB1-TSG-4	LB1-LSG-3	LB1-LSG-4	LB2-TSG-1	LB2-TSG-2	LB2-LSG-1	LB2-LSG-2	LB2-TSG-3	LB2-TSG-4	LB2-LSG-3	LB2-LSG-4	LB3-TSG-1	LB3-TSG-2	LB3-LSG-1	LB3-LSG-2	LB3-TSG-3	LB3-TSG-4	LB3-LSG-3	LB3-LSG-4	LB4-TSG-1	LB4-TSG-2	LB4-LSG-1	LB4-LSG-2	LB4-TSG-3	LB4-LSG-3	LM2-TSG-6	LM2-LSG-6"
        Dim gaugenames() As String
        gaugenames = gauges.Split(vbTab) 'create array of all gauge names
        Dim checkboxname() As Control


        For i = 0 To gaugenames.Length - 1 'for however many different gauges there are
            Dim gaugedata(LengthL - 7) As Single
            Dim gaugedataabs(LengthL - 7) As Single
            Dim gaugedatasum(0) As Single
            gaugedatasum(0) = 0
            gaugedatamax(0) = 0
            gaugedatamax(1) = 0

            '--------------
            Dim gaugename As String = gaugenames(i)
            checkboxname = Me.Controls.Find("CheckBox" & (i + 1), True)
            If checkboxname.Length > 0 AndAlso TypeOf checkboxname(0) Is CheckBox Then 'go ahead if checkbox exists
                Dim cb As CheckBox = DirectCast(checkboxname(0), CheckBox)
                If cb.Checked Then 'processes if checkbox is checked
                    'If gaugedatasum(0) / (LengthL - 7) < 15 Then 'processes and displays data only if passes the absolute value average test
                    cb.Text = gaugename
                    '-----------

                    For j = 0 To LengthL - 7 'for each value of sensor
                        gaugedata(j) = alldata(j + 7, i + 1) - alldata(7, i + 1) 'create single column array for this gauge's data, zero'd

                        'sum absolute values for bad response filtering
                        gaugedataabs(j) = Math.Abs(gaugedata(j)) 'takes absoulute value
                        gaugedatasum(0) += gaugedataabs(j) 'sums all absolute values

                        'find max
                        If gaugedata(j) > gaugedatamax(0) Then
                            gaugedatamax(0) = gaugedata(j) 'value of max for this gauge
                            gaugedatamax(1) = j 'location of max for this gauge
                        End If

                    Next

                    'MessageBox.Show(gaugedatasum(0) / (LengthL - 7))
                    '----------- where checkbox came frommmmmm
                    If gaugename.StartsWith("LM") Then 'if taxiway M curves section

                        'set overall max for LM gauges
                        If gaugedatamax(0) > gaugedatamax(2) Then
                            gaugedatamax(2) = gaugedatamax(0)
                            gaugedatamax(3) = gaugedatamax(1)
                        End If

                        'plot data in chart 1
                        If Chart1.Series.IsUniqueName(gaugename) Then
                            Me.Chart1.Series.Add(gaugename)  'adds series to chart for each sensor
                        End If
                        Me.Chart1.Series(gaugename).ChartType = DataVisualization.Charting.SeriesChartType.Line 'makes series line type chart
                            Me.Chart1.Series(gaugename).Points.DataBindXY(timex, gaugedata) 'plots data

                        'make chart zoomable (not currently working)
                        'Me.Chart1.ChartAreas(0).AxisX.ScaleView.Zoomable = True
                        'Me.Chart1.ChartAreas(0).AxisY.ScaleView.Zoomable = True
                        'Me.Chart1.ChartAreas(0).CursorX.AutoScroll = True
                        'Me.Chart1.ChartAreas(0).CursorY.AutoScroll = True

                    Else 'if taxiway B straight section

                        'set overall max for LB gauges
                        If gaugedatamax(0) > gaugedatamax(4) Then
                            gaugedatamax(4) = gaugedatamax(0)
                            gaugedatamax(5) = gaugedatamax(1)
                        End If

                        'plot data in chart 2
                        If Chart2.Series.IsUniqueName(gaugename) Then
                            Me.Chart2.Series.Add(gaugename)  'adds series to chart for each sensor
                        End If
                        Me.Chart2.Series(gaugename).ChartType = DataVisualization.Charting.SeriesChartType.Line 'makes series line type chart
                        Me.Chart2.Series(gaugename).Points.DataBindXY(timex, gaugedata) 'plots data

                        'make chart zoomable (not currently working)
                        'Me.Chart2.ChartAreas(0).AxisX.ScaleView.Zoomable = True
                        'Me.Chart2.ChartAreas(0).AxisY.ScaleView.Zoomable = True
                        'Me.Chart2.ChartAreas(0).CursorX.AutoScroll = True
                        'Me.Chart2.ChartAreas(0).CursorY.AutoScroll = True

                    End If 'if taxiway curved section

                End If 'if checkbox checked

            End If 'if checkbox exists
        Next 'for each gauge

        'find peaks and zoom
        'If gaugedatamax(0) > 40 Then 'only if max is more than _ micrometers
        If CheckBox57.Checked Then 'if Zoom to Peak box is checked
            Me.Chart1.ChartAreas(0).AxisX.Minimum = timex(gaugedatamax(3)) - 1
            Me.Chart1.ChartAreas(0).AxisX.Maximum = timex(gaugedatamax(3)) + 1
            Me.Chart2.ChartAreas(0).AxisX.Minimum = timex(gaugedatamax(5)) - 1
            Me.Chart2.ChartAreas(0).AxisX.Maximum = timex(gaugedatamax(5)) + 1
            'ElseIf gaugedatamax(0) < 10 Then
            'Me.Chart1.Series(gaugename).Enabled = False
        Else 'zoom all the way out
            Me.Chart1.ChartAreas(0).AxisX.Minimum = timex(0)
            Me.Chart1.ChartAreas(0).AxisX.Maximum = timex(LengthL - 7)
            Me.Chart2.ChartAreas(0).AxisX.Minimum = timex(0)
            Me.Chart2.ChartAreas(0).AxisX.Maximum = timex(LengthL - 7)
        End If

    End Sub


    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        OpenFileDialog1.Title = "Please Select a File"
        OpenFileDialog1.Filter = "Tab Delimited Files|*.tab"
        OpenFileDialog1.InitialDirectory = "C:\Users\augustyns\Documents\20161114 Boston Logan\bos danymic data\"
        OpenFileDialog1.ShowDialog()
    End Sub

    Private Sub OpenFileDialog1_FileOk(sender As Object, e As ComponentModel.CancelEventArgs) Handles OpenFileDialog1.FileOk
        TextBox1.Text = OpenFileDialog1.FileName.ToString()
    End Sub

    Private Sub CheckBox1_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox1.CheckedChanged
        Button1.PerformClick()
    End Sub

    Private Sub CheckBox2_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox2.CheckedChanged
        Button1.PerformClick()
    End Sub

    Private Sub CheckBox3_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox3.CheckedChanged
        Button1.PerformClick()
    End Sub

    Private Sub CheckBox4_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox4.CheckedChanged
        Button1.PerformClick()
    End Sub

    Private Sub CheckBox5_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox5.CheckedChanged
        Button1.PerformClick()
    End Sub

    Private Sub CheckBox6_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox6.CheckedChanged
        Button1.PerformClick()
    End Sub

    Private Sub CheckBox7_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox7.CheckedChanged
        Button1.PerformClick()
    End Sub

    Private Sub CheckBox8_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox8.CheckedChanged
        Button1.PerformClick()
    End Sub

    Private Sub CheckBox9_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox9.CheckedChanged
        Button1.PerformClick()
    End Sub

    Private Sub CheckBox10_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox10.CheckedChanged
        Button1.PerformClick()
    End Sub

    Private Sub CheckBox11_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox11.CheckedChanged
        Button1.PerformClick()
    End Sub

    Private Sub CheckBox12_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox12.CheckedChanged
        Button1.PerformClick()
    End Sub

    Private Sub CheckBox13_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox13.CheckedChanged
        Button1.PerformClick()
    End Sub

    Private Sub CheckBox14_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox14.CheckedChanged
        Button1.PerformClick()
    End Sub

    Private Sub CheckBox15_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox15.CheckedChanged
        Button1.PerformClick()
    End Sub

    Private Sub CheckBox16_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox16.CheckedChanged
        Button1.PerformClick()
    End Sub

    Private Sub CheckBox17_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox17.CheckedChanged
        Button1.PerformClick()
    End Sub

    Private Sub CheckBox18_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox18.CheckedChanged
        Button1.PerformClick()
    End Sub

    Private Sub CheckBox19_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox19.CheckedChanged
        Button1.PerformClick()
    End Sub

    Private Sub CheckBox20_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox20.CheckedChanged
        Button1.PerformClick()
    End Sub

    Private Sub CheckBox21_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox21.CheckedChanged
        Button1.PerformClick()
    End Sub

    Private Sub CheckBox22_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox22.CheckedChanged
        Button1.PerformClick()
    End Sub

    Private Sub CheckBox23_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox23.CheckedChanged
        Button1.PerformClick()
    End Sub

    Private Sub CheckBox24_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox24.CheckedChanged
        Button1.PerformClick()
    End Sub

    Private Sub CheckBox25_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox25.CheckedChanged
        Button1.PerformClick()
    End Sub

    Private Sub CheckBox26_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox26.CheckedChanged
        Button1.PerformClick()
    End Sub

    Private Sub CheckBox27_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox27.CheckedChanged
        Button1.PerformClick()
    End Sub

    Private Sub CheckBox28_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox28.CheckedChanged
        Button1.PerformClick()
    End Sub

    Private Sub CheckBox29_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox29.CheckedChanged
        Button1.PerformClick()
    End Sub

    Private Sub CheckBox30_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox30.CheckedChanged
        Button1.PerformClick()
    End Sub

    Private Sub CheckBox31_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox31.CheckedChanged
        Button1.PerformClick()
    End Sub

    Private Sub CheckBox32_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox32.CheckedChanged
        Button1.PerformClick()
    End Sub

    Private Sub CheckBox33_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox33.CheckedChanged
        Button1.PerformClick()
    End Sub

    Private Sub CheckBox34_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox34.CheckedChanged
        Button1.PerformClick()
    End Sub

    Private Sub CheckBox35_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox35.CheckedChanged
        Button1.PerformClick()
    End Sub

    Private Sub CheckBox36_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox36.CheckedChanged
        Button1.PerformClick()
    End Sub

    Private Sub CheckBox37_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox37.CheckedChanged
        Button1.PerformClick()
    End Sub

    Private Sub CheckBox38_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox38.CheckedChanged
        Button1.PerformClick()
    End Sub

    Private Sub CheckBox39_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox39.CheckedChanged
        Button1.PerformClick()
    End Sub

    Private Sub CheckBox40_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox40.CheckedChanged
        Button1.PerformClick()
    End Sub

    Private Sub CheckBox41_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox41.CheckedChanged
        Button1.PerformClick()
    End Sub

    Private Sub CheckBox42_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox42.CheckedChanged
        Button1.PerformClick()
    End Sub

    Private Sub CheckBox43_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox43.CheckedChanged
        Button1.PerformClick()
    End Sub

    Private Sub CheckBox44_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox44.CheckedChanged
        Button1.PerformClick()
    End Sub

    Private Sub CheckBox45_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox45.CheckedChanged
        Button1.PerformClick()
    End Sub

    Private Sub CheckBox46_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox46.CheckedChanged
        Button1.PerformClick()
    End Sub

    Private Sub CheckBox47_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox47.CheckedChanged
        Button1.PerformClick()
    End Sub

    Private Sub CheckBox48_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox48.CheckedChanged
        Button1.PerformClick()
    End Sub

    Private Sub CheckBox49_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox49.CheckedChanged
        Button1.PerformClick()
    End Sub

    Private Sub CheckBox50_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox50.CheckedChanged
        Button1.PerformClick()
    End Sub

    Private Sub CheckBox51_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox51.CheckedChanged
        Button1.PerformClick()
    End Sub

    Private Sub CheckBox52_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox52.CheckedChanged
        Button1.PerformClick()
    End Sub

    Private Sub CheckBox53_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox53.CheckedChanged
        Button1.PerformClick()
    End Sub

    Private Sub CheckBox54_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox54.CheckedChanged
        Button1.PerformClick()
    End Sub

    Private Sub CheckBox55_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox55.CheckedChanged
        Button1.PerformClick()
    End Sub

    Private Sub CheckBox56_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox56.CheckedChanged
        Button1.PerformClick()
    End Sub

    Private Sub CheckBox57_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox57.CheckedChanged 'zoom to peak
        Button1.PerformClick()

    End Sub
End Class
