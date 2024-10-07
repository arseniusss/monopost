®
4D:\New C#\Monopost\monopost\Monopost.Web\App.xaml.cs
	namespace 	
Monopost
 
. 
Web 
{		 
public

 

partial

 
class

 
App

 
:

 
Application

 *
{ 
public 
static 
IServiceProvider &
ServiceProvider' 6
{7 8
get9 <
;< =
private> E
setF I
;I J
}K L
	protected 
override 
void 
	OnStartup  )
() *
StartupEventArgs* :
e; <
)< =
{ 	
var 
envFilePath 
= 
Path "
." #
Combine# *
(* +
	AppDomain+ 4
.4 5
CurrentDomain5 B
.B C
BaseDirectoryC P
,P Q
$strR `
)` a
;a b
Env 
. 
Load 
( 
envFilePath  
)  !
;! "
var 
connectionString  
=! "
Environment# .
.. /"
GetEnvironmentVariable/ E
(E F
$strF T
)T U
;U V
var 
serviceCollection !
=" #
new$ '
ServiceCollection( 9
(9 :
): ;
;; <
serviceCollection 
. 
AddDbContext *
<* +
AppDbContext+ 7
>7 8
(8 9
options9 @
=>A C
options 
. 
	UseNpgsql !
(! "
connectionString" 2
)2 3
)3 4
;4 5
ServiceProvider 
= 
serviceCollection /
./ 0 
BuildServiceProvider0 D
(D E
)E F
;F G
base 
. 
	OnStartup 
( 
e 
) 
; 
} 	
} 
} Ù
8D:\New C#\Monopost\monopost\Monopost.Web\AssemblyInfo.cs
[ 
assembly 	
:	 

	ThemeInfo 
( &
ResourceDictionaryLocation 
. 
None #
,# $&
ResourceDictionaryLocation 
. 
SourceAssembly -
)

 
]

 í
;D:\New C#\Monopost\monopost\Monopost.Web\MainWindow.xaml.cs
	namespace 	
Monopost
 
. 
Web 
{ 
public 

partial 
class 

MainWindow #
:$ %
Window& ,
{ 
public 

MainWindow 
( 
) 
{ 	
InitializeComponent 
(  
)  !
;! "
} 	
} 
} 