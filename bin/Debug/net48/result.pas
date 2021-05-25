uses System;
uses System.Collections.Generic;
uses System.Linq;
uses System.Text;
uses System.Threading.Tasks;

function Inc(a: Integer; b: string; c: byte): Integer;
begin
  a += b.Length;
	Result := (a + c);
end;

procedure Pr();
begin
  Console.WriteLine('privet');
end;

begin
  var b := 2; //Это комментарий
  var a: real;
  a := b;
  var x, y, z: char;
  var s := 'PascalABC forever';
  var array_example := Arr(1, 2, 3, 4, 5 );
  var m1 := (array_example[2] = array_example[4]);
  Console.WriteLine(m1);
  while ((b <> 5)) do
  begin
      b += 1;
      a *= b;
  end;
  (*
            Console.WriteLine(a);
            *)
  var c := 0;
  for var i := 0 to 5 do
  begin
      c := i;
      b += c;
  end;
  if (a = c) then
  begin
      Console.WriteLine(15);
  end
  else
  begin
      Console.WriteLine(55);
  end;
  var res := Inc(5, 'b', 1);
  Pr();
  var vs := new real[array_example.Length];
  var j := 0;
  foreach var t in array_example do 
  begin
      vs[j] := t;
      j := (j + 1);
      //int k = j;
      //int n = ++k;
      //int l = k++;
      //Console.WriteLine(n);
      //Console.WriteLine(l);
  end;
  if ((a > 5 )and m1) then
  begin

  end;
  if ((b <> 10 )or not m1) then
  begin

  end;
  if ((((a > 5 )and m1)) and not (((b <> 10 )or not m1))) then
  begin

  end;
  for var t := 10 downto 0 do
  begin
      if (t = 5) then
continue;
      if (t = 1) then
break;
      Console.Write(t);
  end;
	var str: string;
	readln(str);
  Console.WriteLine(str);

  var arr1 := Arr(1, 2, 3, 4, 5 ); 
  try
	var i: Integer;
	readln(i);
      Console.WriteLine((arr1[i] / i));
	except
			on DivideByZeroException do
  begin
      Console.WriteLine('Деление на 0');
  end;
			on e: IndexOutOfRangeException do
  begin
      Console.WriteLine(e.Message);
  end;
			on FormatException do
  begin
      Console.WriteLine('Неверный формат кода');
  end;
	end;
end.
