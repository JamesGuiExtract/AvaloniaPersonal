#############################################################################
# I N C L U D E S 
#
# the LatestComponentVersions.mak file being included here counts on $(ProductRootDirName)
# ProductRootDirName defined as a non-null string
#
!include ..\..\..\Common\LatestComponentVersions.mak
!include ..\..\..\..\Rules\Build_FKB\FKBVersion.mak

#############################################################################
# O V E R R I D A B L E S
# 
# Any component version needs to be applied only to the current project space
