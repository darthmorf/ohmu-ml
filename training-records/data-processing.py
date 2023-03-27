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

if __name__ == "__main__":

  mode = "sac"

  # load data
  files = [f for f in listdir("./") if mode in f if "json" in f]
  data = load_jsons(files)

  # fix indexes & find data points
  index = 1
  maxReward = -999999
  minReward =  999999

  csvData = "EpisodeNumber,IterationTime,Reward,CheckpointCount,AverageVelocity,TimedOut\n"
  
  for item in data:
    item["episodeNumber"] = index
    index += 1

    if item["reward"] > maxReward:
      maxReward = item["reward"]

    if item["reward"] < minReward:
      minReward = item["reward"]

    csvData += f'{item["episodeNumber"]},{item["iterationTime"]},{item["reward"]},{item["checkpointCount"]},{item["averageVelocity"]},{item["timedOut"]}\n'

  with open(f"{mode}-data.csv", 'w') as f:
      f.write(csvData)

  print(f'Episodes: {data[len(data)-1]["episodeNumber"]}')
  print(f'Min Reward: {minReward}')
  print(f'Max Reward: {maxReward}')
  print(f'Written data to csv.')

  
