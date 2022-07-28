using Unity.Collections;
using UnityEngine;
using UnityEngine.ParticleSystemJobs;

[ExecuteInEditMode]
public class ParticlePhysics : MonoBehaviour
{
    ParticleSystem ps;
    UpdateParticlesJob job = new UpdateParticlesJob();

    void Start()
    {
        ps = GetComponent<ParticleSystem>();
        //job.blobs = FindObjectsOfType<Blob>();
        job.spherePos = transform.position;
    }

    void OnParticleUpdateJobScheduled()
    {
        job.Schedule(ps);
    }

    struct UpdateParticlesJob : IJobParticleSystem
    {
        //public Vector2[] blobs;
        public Vector3 spherePos;
        public void Execute(ParticleSystemJobData particles)
        {
            var positions = particles.positions;
            var velocities = particles.velocities;
            var sizes = particles.sizes;
            for (int i = 0; i < particles.count; i++)
            {
                if (Mathf.Pow(sizes[i].x + .5f, 2) > (spherePos - positions[i]).sqrMagnitude)
                {
                    velocities[i] = Vector2.zero;
                }
            }
        }
    }
}