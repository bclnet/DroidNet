#include <android/log.h>

#define ALOG_TAG "TAG1"
#define ALOGI(...) ((void)__android_log_print(ANDROID_LOG_INFO, ALOG_TAG, __VA_ARGS__))
#define ALOGW(...) ((void)__android_log_print(ANDROID_LOG_WARN, ALOG_TAG, __VA_ARGS__))
#define ALOGE(...) ((void)__android_log_print(ANDROID_LOG_ERROR, ALOG_TAG, __VA_ARGS__))
//#if _DEBUG
#define ALOGV(...) ((void)__android_log_print(ANDROID_LOG_VERBOSE, ALOG_TAG, __VA_ARGS__))
//#else
//#define ALOGV(...)
//#endif
