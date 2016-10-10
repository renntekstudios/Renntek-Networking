#ifdef _WIN32
#ifndef _WIN_THREADING_H_
#define _WIN_THREADING_H_
#define WIN32_LEAN_AND_MEAN
#include <windows.h>
#include <process.h>
#include <memory>

using namespace std;

class thread
{
private:
    DWORD thread_id;
    HANDLE thread_handle;
public:
    template<class Call>
    static unsigned int __stdcall threadfunc(void* arg)
    {
        unique_ptr<Call> upCall(static_cast<Call*>(arg));
        (*upCall)();
        return (unsigned long)0;
    }

    template<class Function, class... Args>
    explicit thread(Function&& f, Args&&... args)
    {
        typedef decltype(bind(f, args...)) Call;
        Call* call = new Call(bind(f, args...));
        thread_handle = (HANDLE)_beginthreadex(NULL, 0, threadfunc<Call>, (LPVOID)call, 0, (unsigned*)&thread_id);
    }

    bool joinable() const { return thread_handle != 0; }
    bool join() { return join(INFINITE); }
    bool join(int timeout_ms)
    {
        if(!joinable())
            return false;
        WaitForSingleObject(thread_handle, (DWORD)timeout_ms);
        CloseHandle(thread_handle);
        thread_id = 0;
        return true;
    }

    ~thread()
    {
        if(joinable())
            join();
    }

    bool detach()
    {
        if(!joinable())
            return false;
        if(thread_handle != 0)
        {
            CloseHandle(thread_handle);
            thread_handle = 0;
        }
        thread_id = 0;
    }

    void sleep(int ms)
    {
        Sleep((DWORD)ms);
    }
};

#endif
#endif