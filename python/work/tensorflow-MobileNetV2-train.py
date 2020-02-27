import tensorflow as tf
import matplotlib.pyplot as plt
from util.dataloader import dataloader
from model.model_classification_MobileNetV2 import model_classification_MobileNetV2


loader_train = dataloader('C://Github//VTDEEP//python//dataset//animal-train-v1')
loader_validation = dataloader('C://Github/VTDEEP//python//dataset//animal-validation-v1')

classCount = loader_validation.label_count()
validationCount = loader_validation.sample_count()

learning_rate = 0.00003
train_epoch = 500
batch_size = 50
sample_size = loader_train.sample_count()
total_batch = int(sample_size / batch_size)
target_accuracy = 0.90


sess = tf.compat.v1.Session()

model1 = model_classification_MobileNetV2(sess=sess, class_count=1280)

writer = tf.summary.FileWriter("C:/Github/VTDEEP/python/pretrained-model/model_classification_MobileNetV2/tensorboard", sess.graph)
writer.close()


### gloal variable initializer
global_initializer = tf.compat.v1.global_variables_initializer()
print("global_initializer = ", global_initializer.name)

sess.run(global_initializer)

## save model file
saver = tf.compat.v1.train.Saver()
saver.save(sess, 'C:/Github/VTDEEP/python/pretrained-model/model_classification_MobileNetV2/MobileNetV2')
tf.train.write_graph(sess.graph_def, "", "C:/Github/VTDEEP/python/pretrained-model/model_classification_MobileNetV2/MobileNetV2.pbtxt", as_text=True)

print('learning started')

cost_validation_graph = []
accuracy_validation_graph = []

cost_train_graph = []
accuracy_train_graph = []

for epoch in range(train_epoch):
    epoch_train_avg_cost = 0
    epoch_train_avg_accuracy = 0

    loader_train.clear()
    for i in range(total_batch):
        inputs_train, outputs_train = loader_train.load([100*100*3], 1.0, batch_size)
        if inputs_train is None or outputs_train is None:
            loader_train.clear()
            break

        _ = model1.train(inputs_train, outputs_train, True, learning_rate)
        current_accuray, current_cost = model1.get_cost_accuracy(inputs_train, outputs_train, False)
        epoch_train_avg_cost += (current_cost / total_batch)
        epoch_train_avg_accuracy += (current_accuray / total_batch)


    inputs_validation, output_validation = loader_validation.load([100*100*3], 1.0, validationCount)
    loader_validation.clear()


    validation_accuracy, validation_cost = model1.get_cost_accuracy(inputs_validation, output_validation, False)
    result_label = model1.predict(inputs_validation, False)

    print(result_label)
    accuracy_validation_graph.append(validation_accuracy)
    cost_validation_graph.append(validation_cost)

    cost_train_graph.append(epoch_train_avg_cost)
    accuracy_train_graph.append(epoch_train_avg_accuracy)

    print('Epoch : ', '%04d ' %(epoch + 1), ' train_cost=','{:.2f}'.format(epoch_train_avg_cost),' validation_cost=', '{:.2f}'.format(validation_cost), ' train_accuracy=','{:.2f}'.format(epoch_train_avg_accuracy),' validation_accuracy=', '{:.2f}'.format(validation_accuracy))

    if validation_accuracy >= target_accuracy:
        break



##print("#### saver def ####")
##print('Feed this tensor to set the checkpoint filename: ', saver.as_saver_def().filename_tensor_name)
##print('Run this operation to save a checkpoint        : ', saver.as_saver_def().save_tensor_name)
##print('Run this operation to restore a checkpoint     : ', saver.as_saver_def().restore_op_name)
##print("#### saver def ####")

##with open('C:/Github/VTDEEP/python/pretrained-model/model_classification_vt10/vt10.pb', 'wb') as f:
##  f.write(tf.get_default_graph().as_graph_def().SerializeToString())

##tf.train.write_graph(sess.graph, "C:/Github/VTDEEP/python/pretrained-model/model_classification_vt10", "vt10.pbtxt", False)




plt.plot(cost_train_graph)
plt.plot(accuracy_train_graph)
plt.plot(cost_validation_graph)
plt.plot(accuracy_validation_graph)
plt.ylabel('cost_train, accuracy_train, cost_validation, accuracy_validation, ')
plt.legend(['cost_train', 'accuracy_train', 'cost_validation', 'accuracy_validation'], loc='upper left')
plt.savefig('C://Github/VTDEEP/python/pretrained-model/model_classification_MobileNetV2/pre-trained-MobileNetV2-graph.png')
plt.show()

print('Learning finished.')