from os import listdir
import json

def load_jsons(files):
  jsonData = []
  
  for file in files:
    print(file)
    with open(file, 'r') as f:
      loadedFile = json.loads(f.read())
      jsonData = jsonData + loadedFile

  return jsonData


def process_data(mode):
  # load data
  files = [f for f in listdir("./") if mode in f if "json" in f]
  data = load_jsons(files)

  # fix indexes & find data points
  index = 1
  maxReward = -999999
  minReward =  999999
  maxAvgReward = -999999
  minAvgReward =  999999
  maxVelocity = -1
  maxAvgVelocity = -1
  cumulativeTime = 0

  csvData = "EpisodeNumber,IterationTime,AverageIterationTime,CumulativeIterationTime,Reward,AverageReward,CheckpointCount,Velocity,AverageVelocity,TimedOut\n"

  averageRewards = []
  averageVelocities = []
  averageTimes = []
  
  for item in data:
    cumulativeTime += item["iterationTime"]
    averageRewards.append(item["reward"])
    averageReward = sum(averageRewards) / len(averageRewards)
    averageVelocities.append(item["averageVelocity"])
    averageVelocity = sum(averageVelocities) / len(averageVelocities)
    averageTimes.append(item["iterationTime"])
    averageTime = sum(averageTimes) / len(averageTimes)
    
    item["episodeNumber"] = index
    index += 1

    if item["reward"] > maxReward:
      maxReward = item["reward"]

    if item["reward"] < minReward:
      minReward = item["reward"]


    if averageReward > maxAvgReward:
      maxAvgReward = averageReward

    if averageReward < minAvgReward:
      minAvgReward = averageReward


    if item["averageVelocity"] > maxVelocity:
      maxVelocity = item["averageVelocity"]

    if item["averageVelocity"] > maxVelocity:
      maxVelocity = item["averageVelocity"]

    if averageVelocity > maxAvgVelocity:
      maxAvgVelocity = averageVelocity


    csvData += f'{item["episodeNumber"]},{item["iterationTime"]},{averageTime},{cumulativeTime},{item["reward"]},{averageReward},{item["checkpointCount"]},{item["averageVelocity"]},{averageVelocity},{item["timedOut"]}\n'

  with open(f"{mode}-data.csv", 'w') as f:
      f.write(csvData)

  print(f'Episodes: {data[len(data)-1]["episodeNumber"]}')
  print(f'Min Reward: {minReward}')
  print(f'Max Reward: {maxReward}')
  print(f'Min Avg Reward: {minAvgReward}')
  print(f'Max Avg Reward: {maxAvgReward}')
  print(f'Cumulative Iteration: {cumulativeTime}')
  print(f'Max Speed: {maxVelocity}')
  print(f'Max Avg Speed: {maxAvgVelocity}')
  print(f'Written data to csv.\n\n')



if __name__ == "__main__":

  process_data("sac")
  process_data("ppo")

  
  
