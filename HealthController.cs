using UnityEngine;
using System.Collections;

public class HealthController
{
    public float health { get { return _health; } }
    float _health;
    float _startHealth;

    public HealthController(float startHealth = 10f)
    {
        _health = _startHealth = startHealth;
    }

    public void TakeDamage(float damage)
    {
        if (_health > 0)
        {
            if (damage > _health)
            {
                _health = 0;
            }
            else
            {
                _health -= damage;
            }
        }
        if (_health < 0)
        {
            _health = 0;
        }

    }

    public void AddLifePoints(float points)
    {
        if (_health < _startHealth)
        {
            _health += points;
            if (_health > _startHealth)
            {
                _health = _startHealth;
            }
        }

    }

    public void Kill()
    {
        _health = 0;
    }

    public bool IsDead()
    {
        return (_health <= 0);
    }

    public float GetPercentage()
    {
        return (_health * 100) / _startHealth;
    }

    public float GetZeroOneHelth()
    {
        return _health / _startHealth;
    }

    public void Restore()
    {
        _health = _startHealth;
    }


}
