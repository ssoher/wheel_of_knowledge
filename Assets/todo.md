+ Sound effects
+ Particles
+ Animated UI transitions
+ Game win/lose conditions. Idea: When player exhausts a category's all questions that category will be removed from the wheel. Player wins if the whole wheel is destroyed.
+ Stop using ScriptableObject's for questions/answers and use https://opentdb.com/ API, parse from json and create categories, questions, answers dynamically
+ Question UI is set with a limited amount of answers per question (set to 3 right now), but the QuestionObject supports unlimited amount of answers. The answers UI should be dynamically created under a GridLayout so that questions can support variable amount of answers as set up in the QuestionObject. This way we can control difficulty. More answers = more difficult.
+ Mark questions with difficulty, add difficulty selection based on that, filter questions