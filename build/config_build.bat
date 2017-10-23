set ROOT_DIR=%~dp0

:: Set variable DATETIME to current date and time
bin\date.exe +"%%Y-%%m-%%d-%%H%%M%%S" >.datetime.txt
set /p DATETIME=<.datetime.txt
del .datetime.txt

:: Set variable SVN_REVISION to current revision
svn info --show-item revision >.svn_revision.txt
set /p SVN_REVISION=<.svn_revision.txt
del .svn_revision.txt
