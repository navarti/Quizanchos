﻿namespace Quizanchos.Domain.Entities.Interfaces;

public interface IKeyedEntity<T>
{
    public T Id { get; set; }
}
