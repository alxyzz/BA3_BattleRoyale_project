using System.Collections;
using System.Collections.Generic;

public interface IObserver
{
    void UpdateState(ISubject subject);
}

public interface ISubject
{
    void Attach(IObserver observer);

    void Detach(IObserver observer);

    void Notify();
}
