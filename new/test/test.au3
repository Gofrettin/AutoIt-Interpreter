Local $aArray[2] = [1, "Example"]
; Local $mMap[]
Local $dBinary = Binary("0x00204060")
Local $bBoolean = False
Local $pPtr = Ptr(-1)
Local $hWnd = WinGetHandle(AutoItWinGetTitle())
Local $iInt = 1
Local $fFloat = 2.0
Local $oObject = ObjCreate("Scripting.Dictionary")
Local $sString = "Some text"
Local $tStruct = DllStructCreate("wchar[256]")
Local $vKeyword = Default
Local $fuFunc = ConsoleWrite
Local $fuUserFunc = Test

ConsoleWrite( _
        "Variable Types" & @CRLF & _
        "    $aArray: " & VarGetType($aArray) & " variable type." & @CRLF & _; "$mMap : " & @TAB & @TAB & VarGetType($mMap) & " variable type." & @CRLF & _
        "   $dBinary: " & VarGetType($dBinary) & " variable type." & @CRLF & _
        "  $bBoolean: " & VarGetType($bBoolean) & " variable type." & @CRLF & _
        "      $pPtr: " & VarGetType($pPtr) & " variable type." & @CRLF & _
        "      $hWnd: " & VarGetType($hWnd) & " variable type." & @CRLF & _
        "      $iInt: " & VarGetType($iInt) & " variable type." & @CRLF & _
        "    $fFloat: " & VarGetType($fFloat) & " variable type." & @CRLF & _
        "   $oObject: " & VarGetType($oObject) & " variable type." & @CRLF & _
        "   $sString: " & VarGetType($sString) & " variable type." & @CRLF & _
        "   $tStruct: " & VarGetType($tStruct) & " variable type." & @CRLF & _
        "  $vKeyword: " & VarGetType($vKeyword) & " variable type." & @CRLF & _
        "     MsgBox: " & VarGetType(MsgBox) & " variable type." & @CRLF & _
        "    $fuFunc: " & VarGetType($fuFunc) & " variable type." & @CRLF & _
        "Func 'Test': " & VarGetType(Test) & " variable type." & @CRLF & _
        "$fuUserFunc: " & VarGetType($fuUserFunc) & " variable type.")
Func Test()
EndFunc

Exit

ClipPut('top " kek | jej ')
ConsoleWrite(@OSVersion)
