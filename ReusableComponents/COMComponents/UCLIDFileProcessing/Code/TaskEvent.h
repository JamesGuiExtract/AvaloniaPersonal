#pragma once

enum TaskEventType
{
	kAddTask,
	kUpdateTask,
	kRemoveTask
};

class TaskEvent
{
public:
	TaskEvent(){}
	TaskEvent(const FileProcessingRecord& task, TaskEventType eType) : m_task1(task), m_eType(eType){}
	TaskEvent(const FileProcessingRecord& task1, const FileProcessingRecord& task2, TaskEventType eType) : m_task1(task1), m_task2(task2), m_eType(eType){}

	TaskEventType m_eType;
	FileProcessingRecord m_task1;
	FileProcessingRecord m_task2;
};