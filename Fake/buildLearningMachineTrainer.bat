call fake run build.fsx -t LearningMachineTrainerBuild.Build
call fake run buildLearningMachineTrainer.fsx -t All.Build -p 8
pause
