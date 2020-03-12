import tensorflow as tf
import matplotlib.pyplot as plt
import cv2 as cv2
import numpy as np

from model.model_anomalydetection_SSMIAUTOV2 import model_anomalydetection_SSMIAUTOV2
from util.anogan_dataloader import anogan_dataloader

train_loader = anogan_dataloader('C://Github//OpenDL//python//dataset//woodPatch//')
validation_loader = anogan_dataloader('C://Github//OpenDL//python//dataset//woodPatch//')

train_epoch = 200
batch_size = 20
sample_size = train_loader.size()
total_batch = int(sample_size / batch_size)
target_accuracy = 0.90
learning_rate = 0.0002

sess = tf.Session()
model = model_anomalydetection_SSMIAUTOV2(sess=sess, name="model_anomalydetection_SSMIAUTOV2")

global_variable_initializer = tf.compat.v1.global_variables_initializer()
#local_variable_initializer = tf.compat.v1.local_variables_initializer()
#init_op = tf.group(global_variable_initializer,local_variable_initializer)

print('global variable initializer name : ', global_variable_initializer.name)
sess.run(global_variable_initializer)

## save model file
#saver = tf.compat.v1.train.Saver()
#saver.save(sess, 'C:/Github/OpenDL/python/pretrained-model/model_segmentation_HySegnetV1/model_segmentation_HySegnetV1')
#tf.train.write_graph(sess.graph_def, "", 'C:/Github/OpenDL/python/pretrained-model/model_segmentation_HySegnetV1/model_segmentation_HySegnetV1.pbtxt', as_text=True)


loss_graph = []
accuracy_graph = []
print('Learning start.')

for step in range(train_epoch):

    train_loader.clear()
    total_loss = 0
    generator_total_loss = 0
    avg_accuracy = 0
    accuracy = 0
    train_loader.shuffle()
    for batch in range(total_batch):
        #train_loader.clear()
        input_images = train_loader.load(batch=batch_size, width=128, height=128, shape=[128, 128, 1], resizeWidth=128, resizeHeight=128)

        if input_images is None:
            train_loader.clear()
            break

        _, loss = model.train(input_images, keep_prop=True, learning_rate=learning_rate)
        total_loss += loss / total_batch


    validation_loader.clear()
    validation_loader.shuffle()
    for validation_step in range(validation_loader.size()):
        validation_images = validation_loader.load(batch=1, width=128, height=128, shape=[128, 128, 1], resizeWidth=128, resizeHeight=128)
        accuracy = model.get_accuracy(validation_images, keep_prop=False)
        avg_accuracy += (accuracy / validation_loader.size())
        #print('Validation Step: ', '%04d' % (validation_step + 1), ' step accuracy = ', '{:.9f}'.format(accuracy),)

    validation_test=[]
    validation_test.append(validation_images[0])
    output_images = model.reconstruct(validation_test, keep_prop=False)
    output_reshape = (output_images[0]) * 255
    input_image = (validation_test[0]) * 255


    cv2.imshow('reconstruced image', output_reshape.astype(np.uint8))
    cv2.imshow('input image', input_image.astype(np.uint8))
    cv2.waitKey(10)
    validation_loader.clear()

    accuracy_graph.append(accuracy)
    loss_graph.append(total_loss)


    print('Epoch : ', '%04d' % (step + 1),
          'loss =', '{:.9f}'.format(total_loss),
          'accuracy =', '{:.9f}'.format(avg_accuracy))
    if avg_accuracy > target_accuracy:
       break;

tf.train.write_graph(sess.graph.as_graph_def(),"C:/Github/OpenDL/python/pretrained-model/model_anomalydetection_SSMIAUTO/", "model_anomalydetection_SSMIAUTO.pb")
saver = tf.train.Saver(tf.global_variables())
saver.save(sess, 'C:/Github/OpenDL/python/pretrained-model/model_anomalydetection_SSMIAUTO/model_anomalydetection_SSMIAUTO')

plt.plot(loss_graph)
plt.plot(accuracy_graph)
plt.ylabel('discriminator_loss, generator_loss,  accuracy')
plt.legend(['discriminator_loss', 'generator_loss', 'accuracy'], loc='upper left')
plt.savefig('C:/Github/OpenDL/python/pretrained-model/model_anomalydetection_SSMIAUTO/model_anomalydetection_SSMIAUTO.png')
plt.show()

print('Learning finished.')