/*
 * KdTree.cs
 * RVO2 Library C#
 *
 * Copyright 2008 University of North Carolina at Chapel Hill
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
 * Please send all bug reports to <geom@cs.unc.edu>.
 *
 * The authors may be contacted via:
 *
 * Jur van den Berg, Stephen J. Guy, Jamie Snape, Ming C. Lin, Dinesh Manocha
 * Dept. of Computer Science
 * 201 S. Columbia St.
 * Frederick P. Brooks, Jr. Computer Science Bldg.
 * Chapel Hill, N.C. 27599-3175
 * United States of America
 *
 * <http://gamma.cs.unc.edu/RVO2/>
 */

using System.Collections.Generic;
using System;

namespace RVO
{
    /**
     * <summary>Defines k-D trees for agents and static obstacles in the
     * simulation.</summary>
     */
    public class KdTree
    {
        /**
         * <summary>Defines a node of an agent k-D tree.</summary>
         */
        private struct AgentTreeNode
        {
            internal int begin_;
            internal int end_;
            internal int left_;
            internal int right_;
            internal float maxX_;
            internal float maxY_;
            internal float minX_;
            internal float minY_;
        }

        /**
         * <summary>Defines a pair of scalar values.</summary>
         */
        private struct FloatPair
        {
            private float a_;
            private float b_;

            /**
             * <summary>Constructs and initializes a pair of scalar
             * values.</summary>
             *
             * <param name="a">The first scalar value.</returns>
             * <param name="b">The second scalar value.</returns>
             */
            internal FloatPair(float a, float b)
            {
                a_ = a;
                b_ = b;
            }

            /**
             * <summary>Returns true if the first pair of scalar values is less
             * than the second pair of scalar values.</summary>
             *
             * <returns>True if the first pair of scalar values is less than the
             * second pair of scalar values.</returns>
             *
             * <param name="pair1">The first pair of scalar values.</param>
             * <param name="pair2">The second pair of scalar values.</param>
             */
            public static bool operator <(FloatPair pair1, FloatPair pair2)
            {
                return pair1.a_ < pair2.a_ || !(pair2.a_ < pair1.a_) && pair1.b_ < pair2.b_;
            }

            /**
             * <summary>Returns true if the first pair of scalar values is less
             * than or equal to the second pair of scalar values.</summary>
             *
             * <returns>True if the first pair of scalar values is less than or
             * equal to the second pair of scalar values.</returns>
             *
             * <param name="pair1">The first pair of scalar values.</param>
             * <param name="pair2">The second pair of scalar values.</param>
             */
            public static bool operator <=(FloatPair pair1, FloatPair pair2)
            {
                return (pair1.a_ == pair2.a_ && pair1.b_ == pair2.b_) || pair1 < pair2;
            }

            /**
             * <summary>Returns true if the first pair of scalar values is
             * greater than the second pair of scalar values.</summary>
             *
             * <returns>True if the first pair of scalar values is greater than
             * the second pair of scalar values.</returns>
             *
             * <param name="pair1">The first pair of scalar values.</param>
             * <param name="pair2">The second pair of scalar values.</param>
             */
            public static bool operator >(FloatPair pair1, FloatPair pair2)
            {
                return !(pair1 <= pair2);
            }

            /**
             * <summary>Returns true if the first pair of scalar values is
             * greater than or equal to the second pair of scalar values.
             * </summary>
             *
             * <returns>True if the first pair of scalar values is greater than
             * or equal to the second pair of scalar values.</returns>
             *
             * <param name="pair1">The first pair of scalar values.</param>
             * <param name="pair2">The second pair of scalar values.</param>
             */
            public static bool operator >=(FloatPair pair1, FloatPair pair2)
            {
                return !(pair1 < pair2);
            }
        }

        /**
         * <summary>Defines a node of an obstacle k-D tree.</summary>
         */
        private class ObstacleTreeNode
        {
            internal IBaseObstacle obstacle_;
            internal ObstacleTreeNode left_;
            internal ObstacleTreeNode right_;
        };

        /**
         * <summary>The maximum size of an agent k-D tree leaf.</summary>
         */
        private const int MAX_LEAF_SIZE = 10;

        private IBaseAgent[] agents_;
        private AgentTreeNode[] agentTree_;
        private ObstacleTreeNode obstacleTree_;

        /**
         * <summary>Builds an agent k-D tree.</summary>
         */
        internal void buildAgentTree()
        {
            if (agents_ == null || agents_.Length != Simulator.Instance.agents_.Count)
            {
                agents_ = new IBaseAgent[SimulationManager.agents.Count];

                for (int i = 0; i < agents_.Length; ++i)
                {
                    agents_[i] = SimulationManager.agents[i];
                }

                agentTree_ = new AgentTreeNode[2 * agents_.Length];

                for (int i = 0; i < agentTree_.Length; ++i)
                {
                    agentTree_[i] = new AgentTreeNode();
                }
            }

            if (agents_.Length != 0)
            {
                buildAgentTreeRecursive(0, agents_.Length, 0);
            }
        }

        /**
         * <summary>Builds an obstacle k-D tree.</summary>
         */
        internal void buildObstacleTree()
        {
            obstacleTree_ = new ObstacleTreeNode();

            IList<IBaseObstacle> obstacles = new List<IBaseObstacle>(Simulator.Instance.obstacles_.Count);

            for (int i = 0; i < Simulator.Instance.obstacles_.Count; ++i)
            {
                obstacles.Add(Simulator.Instance.obstacles_[i]);
            }

            obstacleTree_ = buildObstacleTreeRecursive(obstacles);
        }

        /**
         * <summary>Computes the agent neighbors of the specified agent.
         * </summary>
         *
         * <param name="agent">The agent for which agent neighbors are to be
         * computed.</param>
         * <param name="rangeSq">The squared range around the agent.</param>
         */
        internal void computeAgentNeighbors(IBaseAgent agent, ref float rangeSq)
        {
            queryAgentTreeRecursive(agent, ref rangeSq, 0);
        }

        /**
         * <summary>Computes the obstacle neighbors of the specified agent.
         * </summary>
         *
         * <param name="agent">The agent for which obstacle neighbors are to be
         * computed.</param>
         * <param name="rangeSq">The squared range around the agent.</param>
         */
        internal void computeObstacleNeighbors(IBaseAgent agent, float rangeSq)
        {
            queryObstacleTreeRecursive(agent, rangeSq, obstacleTree_);
        }

        /**
         * <summary>Queries the visibility between two points within a specified
         * radius.</summary>
         *
         * <returns>True if q1 and q2 are mutually visible within the radius;
         * false otherwise.</returns>
         *
         * <param name="q1">The first point between which visibility is to be
         * tested.</param>
         * <param name="q2">The second point between which visibility is to be
         * tested.</param>
         * <param name="radius">The radius within which visibility is to be
         * tested.</param>
         */
        internal bool queryVisibility(Vector2 q1, Vector2 q2, float radius)
        {
            return queryVisibilityRecursive(q1, q2, radius, obstacleTree_);
        }

        internal int queryNearAgent(Vector2 point, float radius)
        {
            float rangeSq = float.MaxValue;
            int agentNo = -1;
            queryAgentTreeRecursive(point, ref rangeSq, ref agentNo, 0);
            if (rangeSq < radius*radius)
                return agentNo;
            return -1;
        }

        /**
         * <summary>Recursive method for building an agent k-D tree.</summary>
         *
         * <param name="begin">The beginning agent k-D tree node node index.
         * </param>
         * <param name="end">The ending agent k-D tree node index.</param>
         * <param name="node">The current agent k-D tree node index.</param>
         */
        private void buildAgentTreeRecursive(int begin, int end, int node)
        {
            agentTree_[node].begin_ = begin;
            agentTree_[node].end_ = end;
            agentTree_[node].minX_ = agentTree_[node].maxX_ = agents_[begin].position.x_;
            agentTree_[node].minY_ = agentTree_[node].maxY_ = agents_[begin].position.y_;

            for (int i = begin + 1; i < end; ++i)
            {
                agentTree_[node].maxX_ = Math.Max(agentTree_[node].maxX_, agents_[i].position.x_);
                agentTree_[node].minX_ = Math.Min(agentTree_[node].minX_, agents_[i].position.x_);
                agentTree_[node].maxY_ = Math.Max(agentTree_[node].maxY_, agents_[i].position.y_);
                agentTree_[node].minY_ = Math.Min(agentTree_[node].minY_, agents_[i].position.y_);
            }

            if (end - begin > MAX_LEAF_SIZE)
            {
                /* No leaf node. */
                bool isVertical = agentTree_[node].maxX_ - agentTree_[node].minX_ > agentTree_[node].maxY_ - agentTree_[node].minY_;
                float splitValue = 0.5f * (isVertical ? agentTree_[node].maxX_ + agentTree_[node].minX_ : agentTree_[node].maxY_ + agentTree_[node].minY_);

                int left = begin;
                int right = end;

                while (left < right)
                {
                    while (left < right && (isVertical ? agents_[left].position.x_ : agents_[left].position.y_) < splitValue)
                    {
                        ++left;
                    }

                    while (right > left && (isVertical ? agents_[right - 1].position.x_ : agents_[right - 1].position.y_) >= splitValue)
                    {
                        --right;
                    }

                    if (left < right)
                    {
                        IBaseAgent tempAgent = agents_[left];
                        agents_[left] = agents_[right - 1];
                        agents_[right - 1] = tempAgent;
                        ++left;
                        --right;
                    }
                }

                int leftSize = left - begin;

                if (leftSize == 0)
                {
                    ++leftSize;
                    ++left;
                    ++right;
                }

                agentTree_[node].left_ = node + 1;
                agentTree_[node].right_ = node + 2 * leftSize;

                buildAgentTreeRecursive(begin, left, agentTree_[node].left_);
                buildAgentTreeRecursive(left, end, agentTree_[node].right_);
            }
        }

        /**
         * <summary>Recursive method for building an obstacle k-D tree.
         * </summary>
         *
         * <returns>An obstacle k-D tree node.</returns>
         *
         * <param name="obstacles">A list of obstacles.</param>
         */
        private ObstacleTreeNode buildObstacleTreeRecursive(IList<IBaseObstacle> obstacles)
        {
            if (obstacles.Count == 0)
            {
                return null;
            }

            ObstacleTreeNode node = new ObstacleTreeNode();

            int optimalSplit = 0;
            int minLeft = obstacles.Count;
            int minRight = obstacles.Count;

            for (int i = 0; i < obstacles.Count; ++i)
            {
                int leftSize = 0;
                int rightSize = 0;

                IBaseObstacle obstacleI1 = obstacles[i];
                IBaseObstacle obstacleI2 = obstacleI1.next;

                /* Compute optimal split node. */
                for (int j = 0; j < obstacles.Count; ++j)
                {
                    if (i == j)
                    {
                        continue;
                    }

                    IBaseObstacle obstacleJ1 = obstacles[j];
                    IBaseObstacle obstacleJ2 = obstacleJ1.next;

                    float j1LeftOfI = RVOMath.leftOf(obstacleI1.point, obstacleI2.point, obstacleJ1.point);
                    float j2LeftOfI = RVOMath.leftOf(obstacleI1.point, obstacleI2.point, obstacleJ2.point);

                    if (j1LeftOfI >= -RVOMath.RVO_EPSILON && j2LeftOfI >= -RVOMath.RVO_EPSILON)
                    {
                        ++leftSize;
                    }
                    else if (j1LeftOfI <= RVOMath.RVO_EPSILON && j2LeftOfI <= RVOMath.RVO_EPSILON)
                    {
                        ++rightSize;
                    }
                    else
                    {
                        ++leftSize;
                        ++rightSize;
                    }

                    if (new FloatPair(Math.Max(leftSize, rightSize), Math.Min(leftSize, rightSize)) >= new FloatPair(Math.Max(minLeft, minRight), Math.Min(minLeft, minRight)))
                    {
                        break;
                    }
                }

                if (new FloatPair(Math.Max(leftSize, rightSize), Math.Min(leftSize, rightSize)) < new FloatPair(Math.Max(minLeft, minRight), Math.Min(minLeft, minRight)))
                {
                    minLeft = leftSize;
                    minRight = rightSize;
                    optimalSplit = i;
                }
            }

            {
                /* Build split node. */
                IList<IBaseObstacle> leftObstacles = new List<IBaseObstacle>(minLeft);

                for (int n = 0; n < minLeft; ++n)
                {
                    leftObstacles.Add(null);
                }

                IList<IBaseObstacle> rightObstacles = new List<IBaseObstacle>(minRight);

                for (int n = 0; n < minRight; ++n)
                {
                    rightObstacles.Add(null);
                }

                int leftCounter = 0;
                int rightCounter = 0;
                int i = optimalSplit;

                IBaseObstacle obstacleI1 = obstacles[i];
                IBaseObstacle obstacleI2 = obstacleI1.next;

                for (int j = 0; j < obstacles.Count; ++j)
                {
                    if (i == j)
                    {
                        continue;
                    }

                    IBaseObstacle obstacleJ1 = obstacles[j];
                    IBaseObstacle obstacleJ2 = obstacleJ1.next;

                    float j1LeftOfI = RVOMath.leftOf(obstacleI1.point, obstacleI2.point, obstacleJ1.point);
                    float j2LeftOfI = RVOMath.leftOf(obstacleI1.point, obstacleI2.point, obstacleJ2.point);

                    if (j1LeftOfI >= -RVOMath.RVO_EPSILON && j2LeftOfI >= -RVOMath.RVO_EPSILON)
                    {
                        leftObstacles[leftCounter++] = obstacles[j];
                    }
                    else if (j1LeftOfI <= RVOMath.RVO_EPSILON && j2LeftOfI <= RVOMath.RVO_EPSILON)
                    {
                        rightObstacles[rightCounter++] = obstacles[j];
                    }
                    else
                    {
                        /* Split obstacle j. */
                        float t = RVOMath.det(obstacleI2.point - obstacleI1.point, obstacleJ1.point - obstacleI1.point) / RVOMath.det(obstacleI2.point - obstacleI1.point, obstacleJ1.point - obstacleJ2.point);

                        Vector2 splitPoint = obstacleJ1.point + t * (obstacleJ2.point - obstacleJ1.point);

                        IBaseObstacle newObstacle = new StaticObstacle();
                        newObstacle.point = splitPoint;
                        newObstacle.previous = obstacleJ1;
                        newObstacle.next = obstacleJ2;
                        newObstacle.convex = true;
                        newObstacle.direction = obstacleJ1.direction;

                        newObstacle.id = Simulator.Instance.obstacles_.Count;

                        Simulator.Instance.obstacles_.Add(newObstacle);

                        obstacleJ1.next = newObstacle;
                        obstacleJ2.previous = newObstacle;

                        if (j1LeftOfI > 0.0f)
                        {
                            leftObstacles[leftCounter++] = obstacleJ1;
                            rightObstacles[rightCounter++] = newObstacle;
                        }
                        else
                        {
                            rightObstacles[rightCounter++] = obstacleJ1;
                            leftObstacles[leftCounter++] = newObstacle;
                        }
                    }
                }

                node.obstacle_ = obstacleI1;
                node.left_ = buildObstacleTreeRecursive(leftObstacles);
                node.right_ = buildObstacleTreeRecursive(rightObstacles);

                return node;
            }
        }

        private void queryAgentTreeRecursive(Vector2 position, ref float rangeSq, ref int agentNo, int node)
        {
            if (agentTree_[node].end_ - agentTree_[node].begin_ <= MAX_LEAF_SIZE)
            {
                for (int i = agentTree_[node].begin_; i < agentTree_[node].end_; ++i)
                {
                    float distSq = RVOMath.absSq(position - agents_[i].position);
                    if (distSq < rangeSq)
                    {
                        rangeSq = distSq;
                        agentNo = agents_[i].id;
                    }
                }
            }
            else
            {
                float distSqLeft = RVOMath.sqr(Math.Max(0.0f, agentTree_[agentTree_[node].left_].minX_ - position.x_)) + RVOMath.sqr(Math.Max(0.0f, position.x_ - agentTree_[agentTree_[node].left_].maxX_)) + RVOMath.sqr(Math.Max(0.0f, agentTree_[agentTree_[node].left_].minY_ - position.y_)) + RVOMath.sqr(Math.Max(0.0f, position.y_ - agentTree_[agentTree_[node].left_].maxY_));
                float distSqRight = RVOMath.sqr(Math.Max(0.0f, agentTree_[agentTree_[node].right_].minX_ - position.x_)) + RVOMath.sqr(Math.Max(0.0f, position.x_ - agentTree_[agentTree_[node].right_].maxX_)) + RVOMath.sqr(Math.Max(0.0f, agentTree_[agentTree_[node].right_].minY_ - position.y_)) + RVOMath.sqr(Math.Max(0.0f, position.y_ - agentTree_[agentTree_[node].right_].maxY_));

                if (distSqLeft < distSqRight)
                {
                    if (distSqLeft < rangeSq)
                    {
                        queryAgentTreeRecursive(position, ref rangeSq, ref agentNo, agentTree_[node].left_);

                        if (distSqRight < rangeSq)
                        {
                            queryAgentTreeRecursive(position, ref rangeSq, ref agentNo, agentTree_[node].right_);
                        }
                    }
                }
                else
                {
                    if (distSqRight < rangeSq)
                    {
                        queryAgentTreeRecursive(position, ref rangeSq, ref agentNo, agentTree_[node].right_);

                        if (distSqLeft < rangeSq)
                        {
                            queryAgentTreeRecursive(position, ref rangeSq, ref agentNo, agentTree_[node].left_);
                        }
                    }
                }

            }
        }

        /**
         * <summary>Recursive method for computing the agent neighbors of the
         * specified agent.</summary>
         *
         * <param name="agent">The agent for which agent neighbors are to be
         * computed.</param>
         * <param name="rangeSq">The squared range around the agent.</param>
         * <param name="node">The current agent k-D tree node index.</param>
         */
        private void queryAgentTreeRecursive(IBaseAgent agent, ref float rangeSq, int node)
        {
            if (agentTree_[node].end_ - agentTree_[node].begin_ <= MAX_LEAF_SIZE)
            {
                for (int i = agentTree_[node].begin_; i < agentTree_[node].end_; ++i)
                {
                    agent.AddAgentNeighbor(agents_[i], ref rangeSq);
                }
            }
            else
            {
                float distSqLeft = RVOMath.sqr(Math.Max(0.0f, agentTree_[agentTree_[node].left_].minX_ - agent.position.x_)) + RVOMath.sqr(Math.Max(0.0f, agent.position.x_ - agentTree_[agentTree_[node].left_].maxX_)) + RVOMath.sqr(Math.Max(0.0f, agentTree_[agentTree_[node].left_].minY_ - agent.position.y_)) + RVOMath.sqr(Math.Max(0.0f, agent.position.y_ - agentTree_[agentTree_[node].left_].maxY_));
                float distSqRight = RVOMath.sqr(Math.Max(0.0f, agentTree_[agentTree_[node].right_].minX_ - agent.position.x_)) + RVOMath.sqr(Math.Max(0.0f, agent.position.x_ - agentTree_[agentTree_[node].right_].maxX_)) + RVOMath.sqr(Math.Max(0.0f, agentTree_[agentTree_[node].right_].minY_ - agent.position.y_)) + RVOMath.sqr(Math.Max(0.0f, agent.position.y_ - agentTree_[agentTree_[node].right_].maxY_));

                if (distSqLeft < distSqRight)
                {
                    if (distSqLeft < rangeSq)
                    {
                        queryAgentTreeRecursive(agent, ref rangeSq, agentTree_[node].left_);

                        if (distSqRight < rangeSq)
                        {
                            queryAgentTreeRecursive(agent, ref rangeSq, agentTree_[node].right_);
                        }
                    }
                }
                else
                {
                    if (distSqRight < rangeSq)
                    {
                        queryAgentTreeRecursive(agent, ref rangeSq, agentTree_[node].right_);

                        if (distSqLeft < rangeSq)
                        {
                            queryAgentTreeRecursive(agent, ref rangeSq, agentTree_[node].left_);
                        }
                    }
                }

            }
        }

        /**
         * <summary>Recursive method for computing the obstacle neighbors of the
         * specified agent.</summary>
         *
         * <param name="agent">The agent for which obstacle neighbors are to be
         * computed.</param>
         * <param name="rangeSq">The squared range around the agent.</param>
         * <param name="node">The current obstacle k-D node.</param>
         */
        private void queryObstacleTreeRecursive(IBaseAgent agent, float rangeSq, ObstacleTreeNode node)
        {
            if (node != null)
            {
                IBaseObstacle obstacle1 = node.obstacle_;
                IBaseObstacle obstacle2 = obstacle1.next;

                float agentLeftOfLine = RVOMath.leftOf(obstacle1.point, obstacle2.point, agent.position);

                queryObstacleTreeRecursive(agent, rangeSq, agentLeftOfLine >= 0.0f ? node.left_ : node.right_);

                float distSqLine = RVOMath.sqr(agentLeftOfLine) / RVOMath.absSq(obstacle2.point - obstacle1.point);

                if (distSqLine < rangeSq)
                {
                    if (agentLeftOfLine < 0.0f)
                    {
                        /*
                         * Try obstacle at this node only if agent is on right side of
                         * obstacle (and can see obstacle).
                         */
                        agent.AddObstacleNeighbor(node.obstacle_, rangeSq);
                    }

                    /* Try other side of line. */
                    queryObstacleTreeRecursive(agent, rangeSq, agentLeftOfLine >= 0.0f ? node.right_ : node.left_);
                }
            }
        }

        /**
         * <summary>Recursive method for querying the visibility between two
         * points within a specified radius.</summary>
         *
         * <returns>True if q1 and q2 are mutually visible within the radius;
         * false otherwise.</returns>
         *
         * <param name="q1">The first point between which visibility is to be
         * tested.</param>
         * <param name="q2">The second point between which visibility is to be
         * tested.</param>
         * <param name="radius">The radius within which visibility is to be
         * tested.</param>
         * <param name="node">The current obstacle k-D node.</param>
         */
        private bool queryVisibilityRecursive(Vector2 q1, Vector2 q2, float radius, ObstacleTreeNode node)
        {
            if (node == null)
            {
                return true;
            }

            IBaseObstacle obstacle1 = node.obstacle_;
            IBaseObstacle obstacle2 = obstacle1.next;

            float q1LeftOfI = RVOMath.leftOf(obstacle1.point, obstacle2.point, q1);
            float q2LeftOfI = RVOMath.leftOf(obstacle1.point, obstacle2.point, q2);
            float invLengthI = 1.0f / RVOMath.absSq(obstacle2.point - obstacle1.point);

            if (q1LeftOfI >= 0.0f && q2LeftOfI >= 0.0f)
            {
                return queryVisibilityRecursive(q1, q2, radius, node.left_) && ((RVOMath.sqr(q1LeftOfI) * invLengthI >= RVOMath.sqr(radius) && RVOMath.sqr(q2LeftOfI) * invLengthI >= RVOMath.sqr(radius)) || queryVisibilityRecursive(q1, q2, radius, node.right_));
            }

            if (q1LeftOfI <= 0.0f && q2LeftOfI <= 0.0f)
            {
                return queryVisibilityRecursive(q1, q2, radius, node.right_) && ((RVOMath.sqr(q1LeftOfI) * invLengthI >= RVOMath.sqr(radius) && RVOMath.sqr(q2LeftOfI) * invLengthI >= RVOMath.sqr(radius)) || queryVisibilityRecursive(q1, q2, radius, node.left_));
            }

            if (q1LeftOfI >= 0.0f && q2LeftOfI <= 0.0f)
            {
                /* One can see through obstacle from left to right. */
                return queryVisibilityRecursive(q1, q2, radius, node.left_) && queryVisibilityRecursive(q1, q2, radius, node.right_);
            }

            float point1LeftOfQ = RVOMath.leftOf(q1, q2, obstacle1.point);
            float point2LeftOfQ = RVOMath.leftOf(q1, q2, obstacle2.point);
            float invLengthQ = 1.0f / RVOMath.absSq(q2 - q1);

            return point1LeftOfQ * point2LeftOfQ >= 0.0f && RVOMath.sqr(point1LeftOfQ) * invLengthQ > RVOMath.sqr(radius) && RVOMath.sqr(point2LeftOfQ) * invLengthQ > RVOMath.sqr(radius) && queryVisibilityRecursive(q1, q2, radius, node.left_) && queryVisibilityRecursive(q1, q2, radius, node.right_);
        }
    }
}
